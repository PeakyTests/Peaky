// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using Its.Recipes;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Peaky
{
    internal class TargetConstraint : TestConstraint
    {
        private readonly ConcurrentDictionary<string, bool> cache =new ConcurrentDictionary<string, bool>();

        public TargetConstraint(TestDefinition testDefinition)
        {
            if (typeof(IApplyToTarget).IsAssignableFrom(testDefinition.TestType))
            {
                TestDefinition = testDefinition;
            }
        }

        protected override bool Match(TestTarget target, HttpRequest _) =>
            Match(target);

        internal bool Match(TestTarget target)
        {
            if (TestDefinition == null)
            {
                return true;
            }

            var key = $"TargetConstraint:({target.Environment}:{target.Application}):{TestDefinition.TestType}";

            Func<string, bool> resolve = _ =>
            {
                var test = target.ResolveDependency(TestDefinition.TestType);

                return test.IfTypeIs<IApplyToTarget>()
                           .Then(t => t.AppliesToTarget(target))
                           .ElseDefault();
            };

            return cache.GetOrAdd(key, resolve);
        }
    }
}