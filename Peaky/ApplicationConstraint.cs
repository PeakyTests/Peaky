// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using Its.Recipes;
using Microsoft.AspNetCore.Http;

namespace Peaky
{
    internal class ApplicationConstraint : TestConstraint
    {
        private readonly ConcurrentDictionary<TestTarget, bool> cachedResults = new ConcurrentDictionary<TestTarget, bool>();

        public ApplicationConstraint(TestDefinition testDefinition)
        {
            if (typeof(IApplyToApplication).IsAssignableFrom(testDefinition.TestType))
            {
                TestDefinition = testDefinition;
            }
        }

        protected override bool Match(TestTarget target, HttpRequest request)
        {
            if (TestDefinition == null)
            {
                return true;
            }
            return cachedResults.GetOrAdd(target,
                                          t => target.ResolveDependency(TestDefinition.TestType)
                                                     .IfTypeIs<IApplyToApplication>()
                                                     .Then(test => test.AppliesToApplication(t.Application))
                                                     .Else(() => false));
        }
    }
}