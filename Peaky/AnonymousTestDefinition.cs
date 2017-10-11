// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.AspNetCore.Mvc;

namespace Peaky
{
    internal class AnonymousTestDefinition : TestDefinition
    {
        private readonly string testName;
        private readonly Func<ActionContext, dynamic> run;

        public AnonymousTestDefinition(string testName, Func<ActionContext, dynamic> run)
        {
            if (testName == null)
            {
                throw new ArgumentNullException(nameof(testName));
            }
            if (run == null)
            {
                throw new ArgumentNullException(nameof(run));
            }
            this.testName = testName;
            this.run = run;
        }

        public override string TestName => testName;

        internal override dynamic Run(ActionContext actionContext, Func<Type, object> resolver)
        {
            return run(actionContext);
        }
    }
}