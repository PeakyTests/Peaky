// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using Its.Recipes;
using Microsoft.Its.Recipes;

namespace Peaky
{
    internal sealed class TestUiHtmlConfigurationAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(
            HttpControllerSettings controllerSettings,
            HttpControllerDescriptor controllerDescriptor)
        {
            var scriptUrl = controllerDescriptor.Configuration
                                                .Properties
                                                .IfContains("Peaky.TestUiUri")
                                                .And()
                                                .IfTypeIs<string>()
                                                .Else(() =>"https://itsmonitoringux.azurewebsites.net/its.log.monitoring.js");

            var libraryUrls = controllerDescriptor.Configuration
                                                  .Properties
                                                  .IfContains("Peaky.TestLibraryUris")
                                                  .And()
                                                  .IfTypeIs<IEnumerable<string>>()
                                                  .Else(() => new string[]{});

            var styleSheet = controllerDescriptor.Configuration
                                                .Properties
                                                .IfContains("Peaky.StyleSheets")
                                                .And()
                                                .IfTypeIs<IEnumerable<string>>()
                                                .Else(() => new string[] { });

            controllerSettings.Formatters.Add(new TestUiScriptFormatter(scriptUrl,libraryUrls, styleSheet));
        }
    }
}