// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using static System.Net.WebUtility;

namespace Peaky;

internal class HtmlTestPageRenderer : IHtmlTestPageRenderer
{
    public HtmlTestPageRenderer(TestTargetRegistry testTargetRegistry, TestDefinitionRegistry testRegistry)
    {
        TestTargetRegistry = testTargetRegistry;
        TestRegistry = testRegistry;
    }

    public TestTargetRegistry TestTargetRegistry { get; }

    public TestDefinitionRegistry TestRegistry { get; }

    public async Task RenderTestListAsync(IReadOnlyList<Test> tests, HttpContext httpContext)
    {
        var response = httpContext.Response;

        await WriteStartDocAsync(response, "Tests");

        foreach (var test in tests)
        {
            await WriteTestAsync(test, response);
        }

        await WriteEndDocAsync(response);
    }

    public async Task RenderTestResultAsync(TestResult result, HttpContext httpContext)
    {
        var response = httpContext.Response;

        await WriteStartDocAsync(response, result.Test.Name);

        var outcome = result.Outcome switch
        {
            TestOutcome.Passed => "&#9989;", // ✅
            TestOutcome.Failed => "&#10060;", // ❌
            TestOutcome.Inconclusive => "&#10067;", // ❓
            TestOutcome.Timeout => "&#8987;", // ⌛
            _ => throw new ArgumentOutOfRangeException()
        };

        await response.WriteAsync(
            $"""
             <div>
                 {outcome} <span class="peaky-test-title"><i>{HtmlEncode(result.Test.Name)}</i></span>
             </div>
             """);

        await response.WriteAsync(
            """
            <div class="peaky-test-result">
            """);

        await response.WriteAsync(
            $"""
              <div>
                 <span class="peaky-property">Duration</span>: {result.Duration}
             </div>
             """);

        await response.WriteAsync(
            $"""
              <div>
                 <span class="peaky-property">Return value</span>:
             </div>
             <div class="peaky-property-value">
                 <code>{HtmlEncode(JsonSerializer.Serialize(result.ReturnValue))}</code>
             </div>
             """);

        await response.WriteAsync(
            $"""
             <div>
                 <span class="peaky-property">Log</span>:
             </div>
             <div class="peaky-property-value">
                 <code>{result.Log}</code>
             </div>
             """);

        await response.WriteAsync(
            $"""
             <div>
                 <span class="peaky-property">Exception</span>:
             </div>
             <div class="peaky-property-value">
                 <code>{result.Exception}</code>
             </div>
             """);

        await response.WriteAsync(
            """
            </div>
            """);

        await WriteEndDocAsync(response);
    }

    static async Task WriteTestAsync(Test test, HttpResponse response)
    {
        await response.WriteAsync(
            $"""
             <details>
                 <summary><span class="peaky-test-title">&#129514;<a href="{HttpUtility.HtmlAttributeEncode(test.Url)}">{test.Name}</a></span></summary>
                 <div style="display:inline-block;margin-left:2em;">
                     <span class="peaky-property">Application</span>: {test.Application}
                     <br/>
                     <span class="peaky-property">Environment</span>: {test.Environment}
                     <br/>
                     <span class="peaky-property">Tags</span>: {string.Join(",", test.Tags)}
                 </div>
             </details>
             """);
    }

    private static async Task WriteStartDocAsync(HttpResponse response, string title)
    {
        await response.WriteAsync(
            $"""
             <!DOCTYPE html>
             <html>
                 <head>
                    <meta charset="UTF-8">
                    <title>Peaky - {title}</title>
                    <style>
                        {Css}
                    </style>
                 </head>
                 <body>
                     <div class="peaky-body">
             """);
    }

    private const string Css =
        """
        .peaky-body {
            font-family: "Segoe UI", "Noto Sans", Helvetica, Arial, sans-serif
        }
        .peaky-property {
            font-weight: bold;
        }
        .peaky-property-value {
            margin-left: 1em;
        }
        .peaky-test-title {
            font-weight: bold;
        }
        .peaky-test-result {
            margin-left: 1.5em;
        }
        .peaky-body code {
            margin: .2em;
        }
        .peaky-body div {
            padding: .2em;
        }
        """;

    private static async Task WriteEndDocAsync(HttpResponse response)
    {
        await response.WriteAsync(
            """
                    </div>
                </body>
            </html>
            """);
    }
}