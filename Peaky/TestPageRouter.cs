// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace Peaky
{
    internal class TestPageRouter : PeakyRouter
    {
        private readonly ITestPageRenderer testPageRenderer;

        public TestPageRouter(
            ITestPageRenderer testPageRenderer,
            string pathBase = "/tests") : base(pathBase)
        {
            this.testPageRenderer = testPageRenderer ?? throw new ArgumentNullException(nameof(testPageRenderer));
        }

        public override async Task RouteAsync(RouteContext context)
        {
            await Task.Yield();

            context.Handler = async _ =>
            {
                await testPageRenderer.Render(context.HttpContext);
            };
        }
    }
}
