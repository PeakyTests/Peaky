// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pocket;
using Xunit;
using Xunit.Abstractions;

namespace Peaky.Tests;

public class TestPageTests : IDisposable
{
    private readonly CompositeDisposable disposables = new();

    public TestPageTests(ITestOutputHelper output)
    {
        disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));
    }

    public void Dispose() => disposables.Dispose();

    [Fact]
    public async Task When_HTML_is_requested_then_the_tests_endpoint_returns_UI_bootstrap_HTML()
    {
        var response = await RequestTestsHtml();

        response.ShouldSucceed();

        var result = await response.Content.ReadAsStringAsync();

        response.Content
                .Headers
                .ContentType
                .ToString()
                .Should()
                .Be("text/html");
        result.Should()
              .StartWith("<!doctype html>");
    }

    [Fact]
    public async Task When_HTML_is_requested_from_tests_endpoint_then_test_list_is_rendered()
    {
        var response = await RequestTestsHtml();

        response.ShouldSucceed();

        var result = await response.Content.ReadAsStringAsync();

        result.Should().Contain(@"<script src=""//peakytests.github.io/Peaky/javascripts/peaky.js?version=");

        throw new NotImplementedException();
    }

    [Fact]
    public async Task When_HTML_is_requested_from_test_endpoint_then_test_is_run_and_result_is_rendered()
    {
        var client = CreateClient();

        var result = await client.GetAsync("http://example.com/tests/production/widgetapi/passing_test_returns_object");

        result.Content.Headers.ContentType.Should().Be("text/html");



        // TODO (When_HTML_is_requested_from_test_endpoint_then_test_is_run_and_result_is_rendered) write test
        throw new NotImplementedException();
    }

    [Fact]
    public async Task Test_page_HTML_can_be_overridden()
    {
        var response = await RequestTestsHtml(services =>
        {
            services.AddTransient<IHtmlTestPageRenderer>(_ => new SubstituteTestPageRenderer("not actually html"));
        });

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Be("not actually html");
    }

    public class SubstituteTestPageRenderer : IHtmlTestPageRenderer
    {
        private readonly string html;

        public SubstituteTestPageRenderer(string html) => this.html = html;

        public async Task RenderTestResult(Peaky.TestResult result, HttpContext httpContext)
        {
            await httpContext.Response.WriteAsync(html);
        }

        public async Task RenderTestList(IReadOnlyList<Peaky.Test> tests, HttpContext httpContext)
        {
            await httpContext.Response.WriteAsync(html);
        }
    }

    private HttpClient CreateClient(Action<IServiceCollection> configureServices = null)
    {
        var peakyService = new PeakyService(
            targets =>
                targets.Add("staging", "widgetapi", new Uri("http://example.widgets.com"))
                       .Add("production", "widgetapi", new Uri("http://example.com"))
                       .Add("staging", "sprocketapi", new Uri("http://staging.example.com"))
                       .Add("production", "sprocketapi", new Uri("http://example.com")),
            configureServices: configureServices);

        disposables.Add(peakyService);

        return peakyService.CreateHttpClient();
    }

    private async Task<HttpResponseMessage> RequestTestsHtml(
        Action<IServiceCollection> configureServices = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/tests/");

        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");

        var response = await CreateClient(configureServices).SendAsync(request);

        return response;
    }
}