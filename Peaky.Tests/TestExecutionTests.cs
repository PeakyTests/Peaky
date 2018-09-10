// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Pocket;
using Xunit;
using Xunit.Abstractions;

namespace Peaky.Tests
{
    public class TestExecutionTests : IDisposable
    {
        private readonly HttpClient apiClient;

        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public TestExecutionTests(ITestOutputHelper output)
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
                    .Add("production",
                         "sprocketapi",
                         new Uri("http://widgets.com"),
                         dependencies => dependencies.Register<HttpClient>(() =>
                         {
                             return new FakeHttpClient(msg => new HttpResponseMessage(HttpStatusCode.OK));
                         })));

            disposables.Add(peakyService);

            apiClient = peakyService.CreateHttpClient();
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task When_a_test_with_a_return_value_passes_then_a_200_is_returned()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/passing_test_returns_object");

            response.ShouldSucceed(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_a_test_with_a_void_return_value_passes_then_a_200_is_returned()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/passing_void_test");

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
        public async Task When_a_test_passes_and_returns_an_object_then_the_response_contains_the_test_return_value()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/passing_test_returns_object");

            var result = await response.Content.ReadAsStringAsync();

            result.Should().Contain("success!");
        }

        [Fact]
        public async Task When_a_test_passes_and_returns_a_struct_then_the_response_contains_the_test_return_value()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/passing_test_returns_struct");

            var result = JsonConvert.DeserializeObject<TestResult>(await response.Content.ReadAsStringAsync());
            result.Passed.Should().BeTrue();
        }

        [Fact]
        public async Task When_a_test_executes_and_returns_a_struct_then_the_response_contains_test_metadata()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/passing_test_returns_struct");

            var result = JsonConvert.DeserializeObject<TestResult>(await response.Content.ReadAsStringAsync());
            result.Test.Should().NotBeNull();
            result.Test.Application.Should().Be("widgetapi");
            result.Test.Environment.Should().Be("production");
            result.Test.Url.Should().Be("http://blammo.com/tests/production/widgetapi/passing_test_returns_struct");
            result.Test.Tags.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task When_a_test_with_a_return_value_throws_then_a_500_Test_Failed_is_returned()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/failing_test");

            response.ShouldFailWith(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task When_a_test_with_a_void_return_value_throws_then_a_500_Test_Failed_is_returned()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/failing_void_test");

            response.ShouldFailWith(HttpStatusCode.InternalServerError);

            (await response.Content
                    .ReadAsStringAsync())
                    .Should()
                    .Contain("oops!");
        }

        [Fact]
        public async Task When_a_test_with_a_Task_return_value_throws_then_a_500_Test_Failed_is_returned()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/failing_void_async_test");

            response.ShouldFailWith(HttpStatusCode.InternalServerError);

            (await response.Content
                    .ReadAsStringAsync())
                    .Should()
                    .Contain("oops!");
        }

        [Fact]
        public async Task When_a_test_with_a_return_value_fails_then_the_response_contains_the_test_return_value_and_exception_details()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/failing_test");

            var result = await response.Content.ReadAsStringAsync();

            result.Should().Contain("oops!");
        }

        [Fact]
        public async Task When_a_test_with_a_void_return_value_fails_then_the_response_contains_the_test_return_value_and_exception_details()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/failing_void_test");

            var result = await response.Content.ReadAsStringAsync();

            result.Should().Contain("oops!");
        }

        [Fact]
        public async Task When_a_test_is_not_valid_for_a_given_environment_then_calling_it_returns_404()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/internal_only_test");

            response.ShouldFailWith(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task When_a_test_is_not_valid_for_a_given_application_then_calling_it_returns_404()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/sprocketapi/widgetapi_only_test");

            response.ShouldFailWith(HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData("tests/production/widgetapi/passing_test_returns_object")]
        [InlineData("TESTS/production/widgetapi/passing_test_returns_object")]
        [InlineData("tests/PRODUCTION/widgetapi/passing_test_returns_object")]
        [InlineData("tests/production/WIDGETAPI/passing_test_returns_object")]
        [InlineData("tests/production/widgetapi/PASSING_TEST_RETURNS_OBJECT")]
        public async Task Test_invocation_is_case_insensitive(string relativeUri)
        {
            var response = await apiClient.GetAsync($"http://blammo.com/{relativeUri}");

            response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Tests_return_a_duration()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/production/widgetapi/passing_test_returns_object");

            var result = await response.AsTestResult();

            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        }
    }
}
