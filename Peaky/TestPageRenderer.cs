using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Peaky
{
    public class TestPageRenderer : ITestPageRenderer
    {
        private static readonly string version = typeof(TestPageRouter)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;

        private readonly string scriptUrl;
        private readonly IEnumerable<PathString> libraryUrls;
        private readonly IEnumerable<PathString> styleSheets;
        private readonly string html;

        public TestPageRenderer(
            string scriptUrl = "//phillippruett.github.io/Peaky/javascripts/peaky.js",
            IEnumerable<PathString> libraryUrls = null,
            IEnumerable<PathString> styleSheets = null)
        {
            if (string.IsNullOrWhiteSpace(scriptUrl))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(scriptUrl));
            }
            this.scriptUrl = scriptUrl;
            this.libraryUrls = libraryUrls;
            this.styleSheets = styleSheets;
            html = Html();
        }

        public async Task Render(HttpContext httpContext)
        {
            await httpContext.Response.WriteAsync(html);
        }

        private string Html()
        {
            var libraryScriptRefs = string.Join("\n",
                                                (libraryUrls ??
                                                 Array.Empty<PathString>())
                                                .Select(u => $@"<script src=""{u}""></script>"));

            var defaultStylesheet =
                new PathString($"//phillippruett.github.io/Peaky/stylesheets/peaky.css")
                    .Add(new QueryString($"?version={version}"));

            var styleSheetRefs =
                string.Join("\n",
                            (styleSheets ??
                             new PathString[] { defaultStylesheet })
                            .Select(u => $@"<link rel=""stylesheet"" href=""{u.Value}"" >"));

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
