using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;

namespace Peaky.SampleWebApplication
{
    public class MicrosoftTests : IApplyToApplication,
                                  IApplyToEnvironment,
                                  IHaveTags
    {
        private readonly HttpClient httpClient;

        public MicrosoftTests(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public bool AppliesToApplication(string application)
        {
            return application == "microsoft";
        }

        public bool AppliesToEnvironment(string environment)
        {
            return environment == "production";
        }

        public string[] Tags => new[]
                                {
                                    "NonSideEffecting"
                                };

        public async Task surface_should_return_200OK()
        {
            (await httpClient.GetAsync("/surface")).StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public async Task windows_should_return_200OK()
        {
            (await httpClient.GetAsync("/en-us/windows")).StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}