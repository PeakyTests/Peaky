using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;

namespace Peaky.SampleWebApplication;

public class BingTests : IApplyToApplication,
                         IHaveTags
{
    private readonly HttpClient httpClient;
    private readonly TestTarget testTarget;

    public BingTests(HttpClient httpClient, TestTarget testTarget)
    {
        this.httpClient = httpClient;
        this.testTarget = testTarget;
    }

    public bool AppliesToApplication(string application)
    {
        return application == "bing";
    }

    public string[] Tags => new[]
    {
        "LiveSite",
        "NonSideEffecting"
    };

    public async Task<string> bing_homepage_returned_in_under_5ms()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        await httpClient.GetAsync("/");
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5);

        return $"{stopwatch.ElapsedMilliseconds} milliseconds";
    }

    public async Task<string> images_should_return_200OK()
    {
        var result = await httpClient.GetAsync("/images");
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await result.Content.ReadAsStringAsync();
        return JsonConvert.SerializeObject(new
        {
            StatusCode = result.StatusCode,
            Content = content
        });
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