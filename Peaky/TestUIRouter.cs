using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Peaky
{
    internal class TestUIRouter : PeakyRouter
    {
        private static readonly string version = typeof(TestUIRouter)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;

        private readonly string html;

        public TestUIRouter(string pathBase = "/tests") : base(pathBase)
        {
            html = InitializeHtml();
        }

        public override async Task RouteAsyncInternal(RouteContext context)
        {
            if (RouteMatches(context))
            {
                context.Handler = async httpContext =>
                {
                    await context.HttpContext.Response.WriteAsync(html);
                };
            }
        }

        private static string InitializeHtml(
            string scriptUrl = "//phillippruett.github.io/Peaky/javascripts/peaky.js",
            IEnumerable<string> libraryUrls = null,
            IEnumerable<string> styleSheets = null)
        {
            var libraryScriptRefs = string.Join("\n", (libraryUrls ?? Array.Empty<string>())
                                                .Select(u => $@"<script src=""{u}""></script>"));

            var styleSheetRefs =
                string.Join("\n", (styleSheets ?? new[] { "//phillippruett.github.io/Peaky/stylesheets/peaky.css" })
                            .Select(u => $@"<link rel=""stylesheet"" href=""{u}"" >"));

            return
                $@"<!doctype html>
<html lang=""en"">
    <head>
	    <meta charset=""UTF-8"">
        {libraryScriptRefs}
        {styleSheetRefs}
    </head>
    <body>
<div id=""container"">
</div>
	    <script src=""{scriptUrl}?version={version}""></script>
    </body>
</html>";
        }
    }
}
