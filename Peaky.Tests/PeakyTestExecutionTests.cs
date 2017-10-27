// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Pocket;
using Xunit;
using Xunit.Abstractions;

namespace Peaky.Tests
{
    public class PeakyTestExecutionTests : IDisposable
    {
        private readonly HttpClient apiClient;

        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public PeakyTestExecutionTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));

            var peakyService = new PeakyService(
                targets => targets
                    .Add("production",
                         "widgetapi",
                         new Uri("http://widgets.com"),
                         dependencies => dependencies.Register<HttpClient>(() => new FakeHttpClient(msg => new HttpResponseMessage(HttpStatusCode.OK))))
                    .Add("staging",
                         "widgetapi",
                         new Uri("http://widgets.com"),
                         dependencies => dependencies.Register<HttpClient>(() => new FakeHttpClient(msg => new HttpResponseMessage(HttpStatusCode.OK))))
                    .Add("production",
                         "sprocketapi",
                         new Uri("http://widgets.com"),
                         dependencies => dependencies.Register<HttpClient>(() => new FakeHttpClient(msg => new HttpResponseMessage(HttpStatusCode.OK)))));

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
        public async Task When_a_test_with_a_return_value_passes_then_a_200_is_returned()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/passing_test_returns_object");

            response.ShouldSucceed(HttpStatusCode.OK);
        }

        [Fact]
        public void When_a_test_with_a_void_return_value_passes_then_a_200_is_returned()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/passing_void_test").Result;

            response.ShouldSucceed(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_a_test_with_a_resultless_Task_return_value_passes_then_a_200_is_returned()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/passing_void_async_test");

            response.ShouldSucceed(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_a_test_with_a_resultless_Task_return_value_passes_then_ReturnValue_is_null()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/passing_void_async_test");
            
            var result = await response.AsTestResult();

            result.ReturnValue.Should().Be(null);
        }

        [Fact]
        public void When_a_test_passes_and_returns_an_object_then_the_response_contains_the_test_return_value()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/passing_test_returns_object").Result;

            var result = response.Content.ReadAsStringAsync().Result;

            result.Should().Contain("success!");
        }

        [Fact]
        public void When_a_test_passes_and_returns_a_struct_then_the_response_contains_the_test_return_value()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/passing_test_returns_struct").Result;

            var result = JsonConvert.DeserializeObject<TestResult>(response.Content.ReadAsStringAsync().Result);

            result.Passed.Should().BeTrue();
        }

        [Fact]
        public void When_a_test_with_a_return_value_throws_then_a_500_Test_Failed_is_returned()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/failing_test").Result;

            response.ShouldFailWith(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void When_a_test_with_a_void_return_value_throws_then_a_500_Test_Failed_is_returned()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/failing_void_test").Result;

            response.ShouldFailWith(HttpStatusCode.InternalServerError);

            response.Content
                    .ReadAsStringAsync()
                    .Result
                    .Should()
                    .Contain("oops!");
        }

        [Fact]
        public void When_a_test_with_a_Task_return_value_throws_then_a_500_Test_Failed_is_returned()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/failing_void_async_test").Result;

            response.ShouldFailWith(HttpStatusCode.InternalServerError);

            response.Content
                    .ReadAsStringAsync()
                    .Result
                    .Should()
                    .Contain("oops!");
        }

        [Fact]
        public void When_a_test_with_a_return_value_fails_then_the_response_contains_the_test_return_value_and_exception_details()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/failing_test").Result;

            var result = response.Content.ReadAsStringAsync().Result;

            result.Should().Contain("oops!");
        }

        [Fact]
        public void When_a_test_with_a_void_return_value_fails_then_the_response_contains_the_test_return_value_and_exception_details()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/failing_void_test").Result;

            var result = response.Content.ReadAsStringAsync().Result;

            result.Should().Contain("oops!");
        }

        [Fact]
        public void When_a_test_is_not_valid_for_a_given_environment_then_calling_it_returns_404()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/internal_only_test").Result;

            response.ShouldFailWith(HttpStatusCode.NotFound);
        }

        [Fact]
        public void When_a_test_is_not_valid_for_a_given_application_then_calling_it_returns_404()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/production/sprocketapi/widgetapi_only_test").Result;

            response.ShouldFailWith(HttpStatusCode.NotFound);
        }

        [Fact]
        public void When_a_test_passes_then_the_response_contains_trace_output_written_by_the_test()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_trace").Result;

            var result = response.Content.ReadAsStringAsync().Result;

            result.Should().Contain("Application = widgetapi | Environment = production");
            result.Should().Contain("...and the response\"");
        }

        [Fact(Skip = "Coming soon")]
        public void When_a_test_passes_then_the_response_contains_its_log_output_written_by_the_test()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_its_log").Result;

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
        public async Task When_a_test_fails_then_the_response_contains_trace_output_written_by_the_test()
        {
            TestsWithTraceOutput.GetResponse = () =>
            {
                throw new Exception("Doh!");
            };

            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_trace");

            var result = await response.AsTestResult();

            result.Log.Should().Contain("Application = widgetapi | Environment = production");
        }

        [Fact(Skip = "Coming soon")]
        public async Task When_a_test_fails_then_the_response_contains_PocketLogger_output_written_by_the_test()
        {
            TestsWithTraceOutput.GetResponse = () =>
            {
                Logger.Log.Info("Doh!");
                throw new Exception("oops!");
            };

            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/write_to_trace");

            var result = await response.Content.ReadAsStringAsync();

            result.Should().Contain("Application = widgetapi | Environment = production");

            throw new NotImplementedException();
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
    }

    public class TestsWithTraceOutput : IPeakyTest
    {
        public static Barrier Barrier;

        public static Func<dynamic> GetResponse;

        private readonly TestTarget target;

        public TestsWithTraceOutput(TestTarget target)
        {
            this.target = target;
        }

        public async Task<dynamic> write_to_trace()
        {
            if (Barrier != null)
            {
                Console.WriteLine("before barrier: " + target);
                await Task.Yield();
                Barrier.SignalAndWait(TimeSpan.FromSeconds(2));
                Console.WriteLine("after barrier: " + target);
            }

            Trace.WriteLine($"Application = {target.Application} | Environment = {target.Environment}");

            return GetResponse();
        }

        public dynamic write_to_pocket_logger()
        {
            using (Logger<TestsWithTraceOutput>.Log.OnEnterAndExit().Info("Testing {target}", target))
            {
            }
            return GetResponse();
        }
    }
}
