// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pocket;
using Xunit;
using Xunit.Abstractions;

namespace Peaky.Tests
{
    public class PeakyTestPageTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public PeakyTestPageTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public void When_HTML_is_requested_then_the_tests_endpoint_returns_UI_bootstrap_HTML()
        {
            var response = RequestTestsHtml();

            response.ShouldSucceed();

            var result = response.Content.ReadAsStringAsync().Result;

            response.Content.Headers.ContentType.Should().Be(new[] { "text/html" });
            result.Should().StartWith(@"<!doctype html>");
        }

        [Fact]
        public async Task When_HTML_is_requested_then_it_contains_a_semantically_versioned_script_link()
        {
            var response = RequestTestsHtml();

            response.ShouldSucceed();

            var result = await response.Content.ReadAsStringAsync();

            result.Should().Contain(@"<script src=""//phillippruett.github.io/Peaky/javascripts/peaky.js?version=");
        }

        [Fact]
        public async Task The_default_test_page_contains_a_css_link()
        {
            var response = RequestTestsHtml();

            response.ShouldSucceed();

            var result = await response.Content.ReadAsStringAsync();

            result.Should().Contain(@"<link rel=""stylesheet"" href=""//phillippruett.github.io/Peaky/stylesheets/peaky.css?version=");

        }

        [Fact]
        public async Task Test_page_HTML_can_be_overridden()
        {
            var response = RequestTestsHtml(services =>
            {
                services.AddTransient<ITestPageRenderer>(c => new SubstituteTestPageRenderer("not actually html"));
            });

            var content = await response.Content.ReadAsStringAsync();

            content.Should().Be("not actually html");
        }

      
        public class SubstituteTestPageRenderer : ITestPageRenderer
        {
            private readonly string html;

            public SubstituteTestPageRenderer(string html) => this.html = html;

            public async Task Render(HttpContext httpContext) => await httpContext.Response.WriteAsync(html);
        }

        private HttpClient CreateClient(Action<IServiceCollection> configureServices)
        {
            var peakyService = new PeakyService(
                targets =>
                    targets.Add("staging", "widgetapi", new Uri("http://staging.widgets.com"))
                           .Add("production", "widgetapi", new Uri("http://widgets.com"))
                           .Add("staging", "sprocketapi", new Uri("http://staging.sprockets.com"))
                           .Add("production", "sprocketapi", new Uri("http://sprockets.com")),
                configureServices: configureServices);

            disposables.Add(peakyService);

            return peakyService.CreateHttpClient();
        }

        private HttpResponseMessage RequestTestsHtml(
            Action<IServiceCollection> configureServices = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://blammo.com/tests/");

            request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");

            var response = CreateClient(configureServices).SendAsync(request).Result;

            return response;
        }
    }
}
