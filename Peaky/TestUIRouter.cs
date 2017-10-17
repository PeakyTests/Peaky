using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Peaky
{
    public class TestUIRouter : IRouter
    {
        private static readonly string version = typeof(TestUIRouter)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;

        private readonly string pathBase;
        private readonly string html;

        public TestUIRouter(string pathBase = "/tests")
        {
            this.pathBase = pathBase ?? throw new ArgumentNullException(nameof(pathBase));

            html = SetHtml();
        }

        public async Task RouteAsync(RouteContext context)
        {
            if (!context.HttpContext.Request.Path.StartsWithSegments(new PathString(pathBase), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            context.Handler = async httpContext =>
            {
                await context.HttpContext.Response.WriteAsync(html);
            };
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context) => null;

        private string SetHtml(
            string scriptUrl = "//phillippruett.github.io/Peaky/javascripts/peaky.js",
            IEnumerable<string> libraryUrls = null,
            IEnumerable<string> styleSheets = null)
        {
            var libraryScriptRefs = string.Join("\n", (libraryUrls ?? Array.Empty<string>()).Select(u => $@"<script src=""{u}""></script>"));
            var styleSheetRefs = string.Join("\n", (styleSheets ?? Array.Empty<string>()).Select(u => $@"<link rel=""stylesheet"" href=""{u}"" >"));

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
