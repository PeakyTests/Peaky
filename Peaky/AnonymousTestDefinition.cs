// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Peaky
{
    internal class AnonymousTestDefinition : TestDefinition
    {
        private readonly string testName;
        private readonly Func<HttpContext, Task<object>> run;

        public AnonymousTestDefinition(string testName, Func<HttpContext, Task<object>> run)
        {
            this.testName = testName ??
                            throw new ArgumentNullException(nameof(testName));
            this.run = run ??
                       throw new ArgumentNullException(nameof(run));
        }

        public override string TestName => testName;

        internal override async Task<object> Run(HttpContext _, Func<Type, object> resolve) =>
            await run(_);
    }
}
