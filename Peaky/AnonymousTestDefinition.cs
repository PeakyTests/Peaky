// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Peaky
{
    internal class AnonymousTestDefinition : TestDefinition
    {
        private readonly Func<HttpContext, Task<object>> run;

        public AnonymousTestDefinition(Func<HttpContext, Task<object>> run) =>
            this.run = run ??
                       throw new ArgumentNullException(nameof(run));

        internal override async Task<object> Run(HttpContext _, Func<Type, object> resolve, TestTarget target) =>
            await run(_);

        public override bool AppliesTo(TestTarget target) => false;
    }
}
