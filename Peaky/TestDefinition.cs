// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Peaky
{
    public abstract class TestDefinition
    {
        private IEnumerable<Parameter> testParameters;
        public abstract string TestName { get; }

        public string RouteName => "Peaky-Test-" + TestName;

        public string RouteTemplate { get; set; }

        public string[] Tags { get; set; }

        internal Type TestType { get; set; }

        internal abstract Task<object> Run(HttpContext httpContext, Func<Type, object> resolve);

        internal static TestDefinition Create(MethodInfo methodInfo)
        {
            var testType = methodInfo.DeclaringType;
            var testDefinitionType = typeof(TestDefinition<>).MakeGenericType(testType);
            var testDefinition = (TestDefinition) Activator.CreateInstance(
                testDefinitionType,
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new object[] { methodInfo },
                null);
            testDefinition.TestType = testType;
            testDefinition.Parameters = methodInfo.GetParameters()
                                                  .Select(p =>
                                                              new Parameter(p.Name, p.DefaultValue));
            return testDefinition;
        }

        internal IEnumerable<Parameter> Parameters
        {
            get => testParameters ??
                   (testParameters = System.Linq.Enumerable.Empty<Parameter>());
            set => testParameters = value;
        }
    }
}
