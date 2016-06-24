using System.Diagnostics;
using System.Net;
using System.Net.Http;
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

        public bool AppliesToApplication(string application)
        {
            return application == "bing";
        }

        public bool AppliesToEnvironment(string environment)
        {
            return environment == "prod";
        }

        public string[] Tags => new[]
                                {
                                    "LiveSite",
                                    "NonSideEffecting"
                                };

        public string bing_homepage_returned_in_under_5ms()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            httpClient.GetAsync("/").Wait();
            stopwatch.Stop();

            stopwatch.ElapsedMilliseconds.Should().BeLessThan(5);

            return $"{stopwatch.ElapsedMilliseconds} milliseconds";
        }

        public async Task images_should_return_200OK()
        {
            (await httpClient.GetAsync("/images")).StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public async Task rewards_should_return_200OK()
        {
            (await httpClient.GetAsync("/rewards/dashboard")).StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public async Task maps_should_return_200OK()
        {
            (await httpClient.GetAsync("/mapspreview")).StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public async Task sign_in_link_is_present()
        {
            var response = await (await httpClient.GetAsync("/")).Content.ReadAsStringAsync();

            response.Should().Contain(">Sign In</span>");
        }
    }
}