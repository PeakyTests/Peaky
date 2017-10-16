// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
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
        public void When_HTML_is_requested_then_it_contains_a_semantically_versioned_script_link()
        {
            var response = RequestTestsHtml();

            var result = response.Content.ReadAsStringAsync().Result;

            result.Should().Contain(@"<script src=""https://itsmonitoringux.azurewebsites.net/its.log.monitoring.js?monitoringVersion=");
        }

        [Fact]
        public void The_script_location_for_the_UI_can_be_configured()
        {
            var response = RequestTestsHtml("//itscdn.azurewebsites.net/monitoring/1.0.0/monitoring.js");

            var result = response.Content.ReadAsStringAsync().Result;

            result.Should().Contain(@"<script src=""//itscdn.azurewebsites.net/monitoring/1.0.0/monitoring.js?monitoringVersion=");
        }

        [Fact]
        public void The_library_script_locations_for_the_UI_can_be_configured()
        {
            var response = RequestTestsHtml(testUiLibraryUrls: new []{ "/jquery.js", "/knockout.js" });

            var result = response.Content.ReadAsStringAsync().Result;

            result.Should().Contain(@"<script src=""/jquery.js""></script>");
            result.Should().Contain(@"<script src=""/knockout.js""></script>");
        }

        private HttpResponseMessage RequestTestsHtml(string testUiScript = null, string[] testUiLibraryUrls = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://blammo.com/tests/");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            var response = apiClient.SendAsync(request).Result;
            return response;
        }
    }
}