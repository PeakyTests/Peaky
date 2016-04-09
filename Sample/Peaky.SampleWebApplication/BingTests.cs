using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;

namespace Peaky.SampleWebApplication
{
    public class BingTests : IApplyToApplication,
                             IApplyToEnvironment,
                             IHaveTags
    {
        private readonly HttpClient httpClient;

        public BingTests(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task bing_is_reachable()
        {
            (await httpClient.GetAsync("/")).StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public string bing_homepage_returned_in_under_5ms()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            httpClient.GetAsync("/").Wait();
            stopwatch.Stop();

            stopwatch.ElapsedMilliseconds.Should().BeLessThan(5);

            return $"{stopwatch.ElapsedMilliseconds} milliseconds";
        }


        public async Task sign_in_link_is_present()
        {
            var response = await (await httpClient.GetAsync("/")).Content.ReadAsStringAsync();

            response.Should().Contain(">Sign In</span>");
        }

        public bool AppliesToApplication(string application)
        {
            return application == "bing";
        }

        public bool AppliesToEnvironment(string environment)
        {
            return environment == "prod";
        }

        public string[] Tags => new[] {"LiveSite", "NonSideEffecting"};
    }
}