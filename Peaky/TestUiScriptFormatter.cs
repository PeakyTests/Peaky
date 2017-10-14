// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Peaky
{
    internal class TestUiScriptFormatter // : MediaTypeFormatter
    {
        private readonly string bootstrapHtml;

        public TestUiScriptFormatter(string scriptUrl, IEnumerable<string> libraryUrls, IEnumerable<string> styleSheets)
        {
            var version = typeof(TestUiScriptFormatter).Assembly
                                                       .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                                       .InformationalVersion;
            var libraryScriptRefs = string.Join("\n", libraryUrls.Select(u => $@"<script src=""{u}""></script>"));
            var styleSheetRefs = string.Join("\n", styleSheets.Select(u => $@"<link rel=""stylesheet"" href=""{u}"" >"));

            bootstrapHtml =
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
	    <script src=""{scriptUrl}?monitoringVersion={version}""></script>
    </body>
</html>";

            //            MediaTypeMappings.Add(
            //                new RequestHeaderMapping("Accept",
            //                                         "text/html",
            //                                         StringComparison.InvariantCultureIgnoreCase,
            //                                         false,
            //                                         "text/html"));
        }

        //        public override bool CanReadType(Type type) => false;
        //
        //        public override bool CanWriteType(Type type) => true;
        //
        //        public override async Task WriteToStreamAsync(
        //            Type type,
        //            object value,
        //            Stream writeStream,
        //            HttpContent content,
        //            TransportContext transportContext,
        //            CancellationToken cancellationToken)
        //        {
        //            var writer = new StreamWriter(writeStream);
        //            await writer.WriteAsync(bootstrapHtml);
        //            await writer.FlushAsync();
        //        }
    }
}
