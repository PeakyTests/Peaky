// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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

    public async Task RenderTestResult(TestResult result, HttpContext httpContext)
    {
        // FIX: (Render) 
    }

    public async Task RenderTestList(IReadOnlyList<Test> tests, HttpContext httpContext)
    {
        httpContext.Response.Headers.Add("Content-Type", "text/html");
        await httpContext.Response.WriteAsync(Html(tests));
    }

    private string Html(IReadOnlyList<Test> tests)
    {
        return
            $"""
             <!doctype html>
             <html>
                 <head>
             	    <meta charset="UTF-8">
                 </head>
                 <body>
                     <div id="container">
                     
                     </div>
                 </body>
             </html>
             """;
    }
}