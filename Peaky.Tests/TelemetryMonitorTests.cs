using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Peaky.Tests
{
    public class TelemetryMonitorTests : IDisposable
    {
        private static HttpClient apiClient;
        private readonly PeakyService peakyService;

        public TelemetryMonitorTests()
        {
            peakyService = new PeakyService(targets => targets.Add("staging", "widgetapi", new Uri("http://staging.widgets.com")));
            apiClient = peakyService.CreateHttpClient();
        }

        public void Dispose() => peakyService.Dispose();

        [Fact(Skip = "Under renovation")]
        public async Task a_request_to_a_telemetry_api_with_a_failed_result_should_contain_the_correct_message()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/staging/widgetapi/no_more_than_10_percent_of_calls_have_failed");

            var resultString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(resultString);

            var result = JsonConvert.DeserializeObject<dynamic>(resultString);

            ((string)result.Exception.Message).Should().Be("Expected a value less than or equal to 10% , but found 50%.");
        }

        [Fact(Skip = "Under renovation")]
        public async Task a_request_to_a_telemetry_api_with_a_failed_result_should_contain_the_related_telemetry_events()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/staging/widgetapi/no_more_than_10_percent_of_calls_have_failed");
            var resultString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(resultString);
            Console.WriteLine(resultString);
            ((bool)result.ReturnValue.RelatedEvents[0].Succeeded).Should().Be(false);
        }

        [Fact(Skip = "Under renovation")]
        public async Task a_request_to_a_telemetry_api_with_a_failed_result_should_contain_the_exception()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/staging/widgetapi/no_more_than_10_percent_of_calls_have_failed");
            var result = await response.Content.ReadAsStringAsync();

            result.Should().Contain("AggregationAssertionException");
        }

        [Fact(Skip = "Under renovation")]
        public async Task a_request_to_a_telemetry_api_with_failed_telemetry_results_should_return_InternalServerError()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/staging/widgetapi/no_more_than_10_percent_of_calls_have_failed");
            response.ShouldFailWith(HttpStatusCode.InternalServerError);
        }

        [Fact(Skip = "Under renovation")]
        public async Task a_request_to_a_telemetry_api_without_failed_telemetry_results_should_return_OK()
        {
            var response = await apiClient.GetAsync("http://blammo.com/tests/staging/widgetapi/telemetry_without_failures");
            response.ShouldFailWith(HttpStatusCode.OK);
        }
    }
}