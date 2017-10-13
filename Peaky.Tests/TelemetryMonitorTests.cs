using System;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Peaky.Tests
{
    [TestFixture]
    public class TelemetryMonitorTests
    {
        private static HttpClient apiClient;

        public TelemetryMonitorTests()
        {
            apiClient = new TestApi(targets => targets.Add("staging", "widgetapi", new Uri("http://staging.widgets.com")));
        }

        [Test]
        public void a_request_to_a_telemetry_api_with_a_failed_result_should_contain_the_correct_message()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/staging/widgetapi/no_more_than_10_percent_of_calls_have_failed").Result;

            var resultString = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine(resultString);

            var result = JsonConvert.DeserializeObject<dynamic>(resultString);

            ((string)result.Exception.Message).Should().Be("Expected a value less than or equal to 10% , but found 50%.");
        }

        [Ignore]
        [Test]
        public void a_request_to_a_telemetry_api_with_a_failed_result_should_contain_the_related_telemetry_events()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/staging/widgetapi/no_more_than_10_percent_of_calls_have_failed").Result;
            var resultString = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(resultString);
            Console.WriteLine(resultString);
            ((bool)result.ReturnValue.RelatedEvents[0].Succeeded).Should().Be(false);
        }

        [Test]
        public void a_request_to_a_telemetry_api_with_a_failed_result_should_contain_the_exception()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/staging/widgetapi/no_more_than_10_percent_of_calls_have_failed").Result;
            var result = response.Content.ReadAsStringAsync().Result;

            result.Should().Contain("AggregationAssertionException");
        }

        [Test]
        public void a_request_to_a_telemetry_api_with_failed_telemetry_results_should_return_InternalServerError()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/staging/widgetapi/no_more_than_10_percent_of_calls_have_failed").Result;
            response.ShouldFailWith(HttpStatusCode.InternalServerError);
        }

        [Test]
        public void a_request_to_a_telemetry_api_without_failed_telemetry_results_should_return_OK()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/staging/widgetapi/telemetry_without_failures").Result;
            response.ShouldFailWith(HttpStatusCode.OK);
        }
    }
}