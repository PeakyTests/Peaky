using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Peaky.Tests
{
    public class SessionTests
    {
        private ITestOutputHelper output;

        public SessionTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task Clients_can_access_session_state_by_sending_a_session_header()
        {
            var sessionId = Guid.NewGuid().ToString();

            using (var peakyService = new PeakyService(targets => targets.Add(
                                                           "production",
                                                           "counter",
                                                           new Uri("http://example.com"),
                                                           t => t.Register<HttpClient>(() => new FakeHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK))
                                                           {
                                                               BaseAddress = new Uri("http://example.com")
                                                           })), testTypes: new[] { typeof(SessionfulTests) }))
            {
                var request = CreateRequest(sessionId);

                var response1 = await peakyService.CreateHttpClient().SendAsync(request);
                var response2 = await peakyService.CreateHttpClient().SendAsync(request);

                var result1 = await response1.AsTestResult();

                output.WriteLine(result1.ReturnValue.ToString());
            }

            // TODO (testname) write test
            throw new NotImplementedException("Test testname is not written yet.");

            HttpRequestMessage CreateRequest(string s)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/tests/production/counter/increment");
                request.Headers.Add("peaky-session", s);
                return request;
            }
        }
    }

    public class SessionfulTests
    {
        private readonly CounterState counterState;

        public SessionfulTests(CounterState counterState)
        {
            this.counterState = counterState ?? throw new ArgumentNullException(nameof(counterState));
        }

        public int increment()
        {
            counterState.Count++;

            return counterState.Count;
        }
    }

    public class CounterState
    {
        public int Count { get; set; }
    }
}
