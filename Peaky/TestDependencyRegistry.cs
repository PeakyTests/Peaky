// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Pocket;

namespace Peaky
{
    public class TestDependencyRegistry
    {
        private Dictionary<MethodInfo, IEnumerable<Parameter>> parametrisedTestCases = new Dictionary<MethodInfo, IEnumerable<Parameter>>();

        internal TestDependencyRegistry(PocketContainer container)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
        }

        internal PocketContainer Container { get; }

        public TestDependencyRegistry Register<T>(Func<T> getDependency)
        {
            Container.Register(typeof(T), c => getDependency());
            return this;
        }

        public TestDependencyRegistry RegisterParameterFor<T>(Expression<Action<T>> testCase)
        {
            var parametrizedTestCase = ExtractParametrizedTestCase(testCase.Body as MethodCallExpression);
            parametrisedTestCases[parametrizedTestCase.method] = parametrizedTestCase.parameters;
            return this;
        }


        public TestDependencyRegistry RegisterParameterFor<T, U>(Expression<Func<T, U>> testCase)
        {
            var parametrizedTestCase = ExtractParametrizedTestCase(testCase.Body as MethodCallExpression);
            parametrisedTestCases[parametrizedTestCase.method] = parametrizedTestCase.parameters;
            return this;
        }

        private static (MethodInfo method, Parameter[] parameters) ExtractParametrizedTestCase(MethodCallExpression expression)
        {
            var body = expression;
            var method = body.Method;
            var values = new List<Parameter>();

            var parameters = body.Method.GetParameters();
            var arguments = parameters.Select((p, i) => new { p.Name, Argument = body.Arguments[i].Reduce() });
            foreach (var argument in arguments)
            {
                var argumentName = argument.Name;
                object data;

                switch (argument.Argument)
                {
                    case ConstantExpression ce:
                        data = ce.Value;
                        break;
                    default:
                        var e = Expression.Lambda(argument.Argument);
                        data = e.Compile().DynamicInvoke();
                        break;
                }

                values.Add(new Parameter(argumentName, data));
            }

            return (method, values.ToArray());
        }

        internal IEnumerable<Parameter> GetParameterSetsFor(MethodInfo method)
        {
            return parametrisedTestCases.TryGetValue(method, out var parameters) ? parameters : null;
        }

        internal IEnumerable<(MethodInfo testMethod, IEnumerable<Parameter> testParameters)> GetParameterSetsFor(Type type)
        {
            return parametrisedTestCases.Where(e => e.Key.DeclaringType == type).Select(e =>(e.Key, e.Value));
        }
    }
    
}
