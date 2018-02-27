using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pocket;
using Xunit;
using Xunit.Abstractions;

namespace Peaky.Tests
{
    public class TestLoggingTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public TestLoggingTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));

            TestsWithTraceOutput.GetResponse = () => "...and the response";
            TestsWithLoggerOutput.GetResponse = () => "...and the response";
        }

        public void Dispose()
        {
            disposables.Dispose();
            TestsWithTraceOutput.Barrier = null;
            TestsWithLoggerOutput.Barrier = null;
        }

        [Fact]
        public void When_a_test_passes_then_the_response_contains_trace_output_written_by_the_test()
        {
            var apiClient = CreatePeakyClient();
            var response = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_trace").Result;

            var result = response.Content.ReadAsStringAsync().Result;

            result.Should().Contain("Application = widgetapi | Environment = production");
            result.Should().Contain("...and the response\"");
        }

        [Fact]
        public void When_a_test_passes_then_the_response_contains_ILogger_output_written_by_the_test()
        {
            var apiClient = CreatePeakyClient(c => c.AddSingleton<ILoggerFactory, LoggerFactory>());
            var response = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_logger").Result;

            var result = response.Content.ReadAsStringAsync().Result;

            result.Should().Contain("Application = widgetapi | Environment = production");
            result.Should().Contain("...and the response\"");
        }

        [Fact]
        public async Task When_a_test_passes_then_the_response_does_not_contains_trace_output_written_by_other_tests()
        {
            TestsWithTraceOutput.Barrier = new Barrier(2);

            var apiClient = CreatePeakyClient();
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
            var apiClient = CreatePeakyClient(c => c.AddSingleton<ILoggerFactory, LoggerFactory>());
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
            var apiClient = CreatePeakyClient();
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_trace");

            var result = await response.AsTestResult();

            result.Log.Should().Contain("Application = widgetapi | Environment = production");
        }

        [Fact]
        public async Task When_a_test_fails_then_the_response_contains_ILogger_output_written_by_the_test()
        {
            TestsWithTraceOutput.GetResponse = () => throw new Exception("oops!");
            var apiClient = CreatePeakyClient(c => c.AddSingleton<ILoggerFactory, LoggerFactory>());
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_logger");

            var result = await response.Content.ReadAsStringAsync();

            result.Should().Contain("Application = widgetapi | Environment = production");
        }

        [Fact]
        public async Task When_a_test_fails_then_the_response_does_not_contains_trace_output_written_by_other_tests()
        {
            TestsWithTraceOutput.Barrier = new Barrier(2);
            TestsWithTraceOutput.GetResponse = () => throw new Exception("oh noes!");
            var apiClient = CreatePeakyClient();
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
            TestsWithLoggerOutput.Barrier = new Barrier(2);
            TestsWithLoggerOutput.GetResponse = () => throw new Exception("oh noes!");
            var apiClient = CreatePeakyClient(c => c.AddSingleton<ILoggerFactory, LoggerFactory>());
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

        private HttpClient CreatePeakyClient(Action<IServiceCollection> configureServices = null)
        {
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
                         })),
                configureServices: configureServices);

            disposables.Add(peakyService);

            return peakyService.CreateHttpClient();
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

    public class TestsWithLoggerOutput : IPeakyTest
    {
        public static Barrier Barrier;

        public static Func<dynamic> GetResponse;

        private readonly TestTarget target;
        private readonly ILogger logger;

        public TestsWithLoggerOutput(TestTarget target, ILoggerFactory loggerFactory)
        {
            this.target = target;
            logger = loggerFactory.CreateLogger<TestsWithLoggerOutput>();
        }

        public async Task<dynamic> write_to_logger()
        {
            if (Barrier != null)
            {
                await Task.Yield();
                Barrier.SignalAndWait(2.Seconds());
            }

            logger.LogInformation($"Application = {target.Application} | Environment = {target.Environment}");

            return GetResponse();
        }
    }
}
