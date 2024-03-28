using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Peaky.SampleWebApplication;

public class HomePageTests : IHaveTags
{
    private readonly HttpClient httpClient;
    private ILogger<HomePageTests> logger;

    public HomePageTests(
        HttpClient httpClient,
        ILoggerFactory loggerFactory)
    {
        if (loggerFactory == null)
        {
            throw new ArgumentNullException(nameof(loggerFactory));
        }

        this.logger = loggerFactory.CreateLogger<HomePageTests>();
        this.httpClient = httpClient;
    }

    public string[] Tags => new[]
    {
        "NonSideEffecting"
    };

    public async Task homepage_should_return_200OK()
    {
        var response = await httpClient.GetAsync("/");

        logger.LogInformation("Response: {response}", response);

        response.StatusCode
                .Should()
                .Be(HttpStatusCode.OK);
    }
}