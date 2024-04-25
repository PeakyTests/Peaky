// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Peaky;

internal class HtmlTestPageRenderer : ITestPageRenderer
{
  
    public async Task Render(HttpContext httpContext)
    {
        httpContext.Response.Headers.Add("Content-Type", "text/html");
        await httpContext.Response.WriteAsync(Html());
    }

    private string Html()
    {
        return
            $"""
             <!doctype html>
             <html lang="en">
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