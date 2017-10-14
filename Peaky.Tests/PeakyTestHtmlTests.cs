// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Peaky.Tests
{
    public class PeakyTestHtmlTests
    {

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

        private static HttpResponseMessage RequestTestsHtml(string testUiScript = null, string [] testUiLibraryUrls = null)
        {
            // FIX: (RequestTestsHtml) 
//           var configuration = new HttpConfiguration();
//            var constraintResolver = new DefaultInlineConstraintResolver();
//            constraintResolver.ConstraintMap.Add("among", typeof(AmongConstraint));
//            configuration.MapHttpAttributeRoutes(constraintResolver);
//            configuration.MapTestRoutes(testUiScriptUrl: testUiScript, testUiLibraryUrls: testUiLibraryUrls);
//            configuration.EnsureInitialized();

         var   apiClient = new PeakyService().CreateHttpClient();
            
            var request = new HttpRequestMessage(HttpMethod.Get, "http://blammo.com/tests/");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            var response = apiClient.SendAsync(request).Result;
            return response;
        }
    }
}