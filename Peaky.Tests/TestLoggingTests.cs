using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Pocket;
using Xunit;
using Xunit.Abstractions;

namespace Peaky.Tests
{
    public class TestLoggingTests
    {
        private readonly HttpClient apiClient;

        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public TestLoggingTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));

            var peakyService = new PeakyService(
                targets => targets
                    .Add("production",
                         "widgetapi",
                         new Uri("http://widgets.com"),
                         dependencies => dependencies.Register<HttpClient>(() =>
                         {
                             return new FakeHttpClient(msg => new HttpResponseMessage(HttpStatusCode.OK));
                         }))
                    .Add("staging",
                         "widgetapi",
                         new Uri("http://widgets.com"),
                         dependencies => dependencies.Register<HttpClient>(() =>
                         {
                             return new FakeHttpClient(msg => new HttpResponseMessage(HttpStatusCode.OK));
                         })));

            disposables.Add(peakyService);

            apiClient = peakyService.CreateHttpClient();

            TestsWithTraceOutput.GetResponse = () => "...and the response";
        }

        public void Dispose()
        {
            disposables.Dispose();
            TestsWithTraceOutput.Barrier = null;
        }

        [Fact]
        public void When_a_test_passes_then_the_response_contains_trace_output_written_by_the_test()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_trace").Result;

            var result = response.Content.ReadAsStringAsync().Result;

            result.Should().Contain("Application = widgetapi | Environment = production");
            result.Should().Contain("...and the response\"");
        }

        [Fact]
        public void When_a_test_passes_then_the_response_contains_ILogger_output_written_by_the_test()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_logger").Result;

            var result = response.Content.ReadAsStringAsync().Result;

            result.Should().Contain("Application = widgetapi | Environment = production");
            result.Should().Contain("...and the response\"");
        }

        [Fact]
        public async Task When_a_test_passes_then_the_response_does_not_contains_trace_output_written_by_other_tests()
        {
            TestsWithTraceOutput.Barrier = new Barrier(2);

            var productionResponse = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_trace");
            var stagingResponse = apiClient.GetAsync("http://blammo.com/tests/staging/widgetapi/write_to_trace");

            var productionResult = await productionResponse.Result.Content.ReadAsStringAsync();
            var stagingResult = await stagingResponse.Result.Content.ReadAsStringAsync();

            productionResult.Should().Contain("Environment = production");
            productionResult.Should().NotContain("Environment = staging");
            stagingResult.Should().Contain("Environment = staging");
            stagingResult.Should().NotContain("Environment = production");
        }

        [Fact]
        public async Task When_a_test_passes_then_the_response_does_not_contain_ILogger_output_written_by_other_tests()
        {
            TestsWithTraceOutput.Barrier = new Barrier(2);

            var productionResponse = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_logger");
            var stagingResponse = apiClient.GetAsync("http://blammo.com/tests/staging/widgetapi/write_to_logger");

            var productionResult = await productionResponse.Result.Content.ReadAsStringAsync();
            var stagingResult = await stagingResponse.Result.Content.ReadAsStringAsync();

            productionResult.Should().Contain("Environment = production");
            productionResult.Should().NotContain("Environment = staging");
            stagingResult.Should().Contain("Environment = staging");
            stagingResult.Should().NotContain("Environment = production");
        }

        [Fact]
        public async Task When_a_test_fails_then_the_response_contains_trace_output_written_by_the_test()
        {
            TestsWithTraceOutput.GetResponse = () => throw new Exception("Doh!");

            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_trace");

            var result = await response.AsTestResult();

            result.Log.Should().Contain("Application = widgetapi | Environment = production");
        }

        [Fact]
        public async Task When_a_test_fails_then_the_response_contains_ILogger_output_written_by_the_test()
        {
            TestsWithTraceOutput.GetResponse = () => throw new Exception("oops!");

            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_logger");

            var result = await response.Content.ReadAsStringAsync();

            result.Should().Contain("Application = widgetapi | Environment = production");
        }

        [Fact]
        public async Task When_a_test_fails_then_the_response_does_not_contains_trace_output_written_by_other_tests()
        {
            TestsWithTraceOutput.Barrier = new Barrier(2);
            TestsWithTraceOutput.GetResponse = () => throw new Exception("oh noes!");

            var productionResponse = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_trace");
            var stagingResponse = apiClient.GetAsync("http://blammo.com/tests/staging/widgetapi/write_to_trace");

            var productionResult = await productionResponse.Result.Content.ReadAsStringAsync();
            var stagingResult = await stagingResponse.Result.Content.ReadAsStringAsync();

            productionResult.Should().Contain("Environment = production");
            productionResult.Should().NotContain("Environment = staging");
            stagingResult.Should().Contain("Environment = staging");
            stagingResult.Should().NotContain("Environment = production");
            productionResult.Should().Contain("oh noes!");
            stagingResult.Should().Contain("oh noes!");
        }

        [Fact]
        public async Task When_a_test_fails_then_the_response_does_not_contain_ILogger_output_written_by_other_tests()
        {
            TestsWithTraceOutput.Barrier = new Barrier(2);
            TestsWithTraceOutput.GetResponse = () => throw new Exception("oh noes!");

            var productionResponse = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_logger");
            var stagingResponse = apiClient.GetAsync("http://blammo.com/tests/staging/widgetapi/write_to_logger");

            var productionResult = await productionResponse.Result.Content.ReadAsStringAsync();
            var stagingResult = await stagingResponse.Result.Content.ReadAsStringAsync();

            productionResult.Should().Contain("Environment = production");
            productionResult.Should().NotContain("Environment = staging");
            stagingResult.Should().Contain("Environment = staging");
            stagingResult.Should().NotContain("Environment = production");
            productionResult.Should().Contain("oh noes!");
            stagingResult.Should().Contain("oh noes!");
        }
    }

    public class TestsWithTraceOutput : IPeakyTest
    {
        public static Barrier Barrier;

        public static Func<dynamic> GetResponse;

        private readonly TestTarget target;

        public TestsWithTraceOutput(TestTarget target) => this.target = target;

        public async Task<dynamic> write_to_trace()
        {
            if (Barrier != null)
            {
                await Task.Yield();
                Barrier.SignalAndWait(TimeSpan.FromSeconds(2));
            }

            Trace.WriteLine($"Application = {target.Application} | Environment = {target.Environment}");

            return GetResponse();
        }
    }

    public class TestsWithLoggeOutput : IPeakyTest
    {
        public static Barrier Barrier;

        public static Func<dynamic> GetResponse;

        private readonly TestTarget target;
        private readonly ILogger logger;

        public TestsWithLoggeOutput(TestTarget target, ILogger logger)
        {
            this.target = target;
            this.logger = logger;
        }

        public async Task<dynamic> write_to_logger()
        {
            if (Barrier != null)
            {
                await Task.Yield();
                Barrier.SignalAndWait(TimeSpan.FromSeconds(2));
            }

            Trace.WriteLine($"Application = {target.Application} | Environment = {target.Environment}");

            return GetResponse();
        }
    }
}
