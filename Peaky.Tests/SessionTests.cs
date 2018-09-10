using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Peaky.Tests
{
    public class SessionTests
    {
        [Fact]
        public async Task Repeat_requests_can_access_persistent_session_state()
        {
            using (var peakyService = CreatePeakyService())
            {
                var client = peakyService.CreateHttpClient();

                var response1 = await client.SendAsync(CounterRequest());
                var response2 = await client.SendAsync(CounterRequest());

                var result1 = (long) (await response1.AsTestResult()).ReturnValue;
                var result2 = (long) (await response2.AsTestResult()).ReturnValue;

                result2.Should().Be(result1 + 1);
            }
        }

        [Fact]
        public async Task Tests_for_different_applications_and_environments_can_share_session_data()
        {
            using (var peakyService = CreatePeakyService())
            {
                var client = peakyService.CreateHttpClient();

                var response1 = await client.SendAsync(CounterRequest("production"));
                var response2 = await client.SendAsync(CounterRequest("staging"));

                var result1 = (long) (await response1.AsTestResult()).ReturnValue;
                var result2 = (long) (await response2.AsTestResult()).ReturnValue;

                result2.Should().Be(result1 + 1);
            }
        }

        [Fact]
        public async Task When_a_different_session_used_then_a_different_state_is_accessed()
        {
            using (var peakyService = CreatePeakyService())
            {
                var response1 = await peakyService
                                      .CreateHttpClient()
                                      .SendAsync(CounterRequest());
                var response2 = await peakyService
                                      .CreateHttpClient()
                                      .SendAsync(CounterRequest());

                var result1 = (long) (await response1.AsTestResult()).ReturnValue;
                var result2 = (long) (await response2.AsTestResult()).ReturnValue;

                result1.Should().Be(1);
                result2.Should().Be(1);
            }
        }

        private HttpRequestMessage CounterRequest(string environment = "production") =>
            new HttpRequestMessage(HttpMethod.Get,
                                   $"http://example.com/tests/{environment}/counter/increment");

        private static PeakyService CreatePeakyService()
        {
            return new PeakyService(
                targets => targets
                           .Add(
                               "production",
                               "counter",
                               new Uri("http://example.com"))
                           .Add(
                               "staging",
                               "counter",
                               new Uri("http://example.com")),
                testTypes: new[] { typeof(Counter) });
        }
    }

    public class Counter : IPeakyTest
    {
        private readonly TestSession session;

        public Counter(TestSession session)
        {
            this.session = session;
        }

        public int Increment()
        {
            var counterString = session.GetString("counter");

            var counter = string.IsNullOrEmpty(counterString)
                              ? 0
                              : int.Parse(counterString);

            counter++;

            session.SetString("counter", counter.ToString());

            return counter;
        }
    }
}
