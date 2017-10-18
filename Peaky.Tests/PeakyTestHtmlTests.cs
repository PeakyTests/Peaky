// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Pocket;
using Xunit;
using Xunit.Abstractions;

namespace Peaky.Tests
{
    public class PeakyTestHtmlTests : IDisposable
    {
        private readonly HttpClient apiClient;
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public PeakyTestHtmlTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));

            var peakyService = new PeakyService(
                targets =>
                    targets.Add("staging", "widgetapi", new Uri("http://staging.widgets.com"))
                           .Add("production", "widgetapi", new Uri("http://widgets.com"))
                           .Add("staging", "sprocketapi", new Uri("http://staging.sprockets.com"))
                           .Add("production", "sprocketapi", new Uri("http://sprockets.com")));

            apiClient = peakyService.CreateHttpClient();

            disposables.Add(apiClient);
            disposables.Add(peakyService);
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public void When_HTML_is_requested_then_the_tests_endpoint_returns_UI_bootstrap_HTML()
        {
            var response = RequestTestsHtml();

            var result = response.Content.ReadAsStringAsync().Result;

            result.Should().StartWith(@"<!doctype html>");
        }

        [Fact]
        public async Task When_HTML_is_requested_then_it_contains_a_semantically_versioned_script_link()
        {
            var response = RequestTestsHtml();

            var result = await response.Content.ReadAsStringAsync();

            result.Should().Contain(@"<script src=""//phillippruett.github.io/Peaky/javascripts/peaky.js?version=");
        }

        [Fact]
        public void testname()
        {
            var headerValue = MediaTypeWithQualityHeaderValue.Parse("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");

            // TODO (testname) write test
            throw new NotImplementedException("Test testname is not written yet.");
        }

        private HttpResponseMessage RequestTestsHtml()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://blammo.com/tests/");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/html"));
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xhtml+xml"));
            var response = apiClient.SendAsync(request).Result;
            return response;
        }
    }
}