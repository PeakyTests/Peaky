// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Pocket;

namespace Peaky
{
    public class TestDependencyRegistry
    {
        private readonly ConcurrentDictionary<MethodInfo, ConcurrentBag<IEnumerable<Parameter>>> parametrizedTestCases = new ConcurrentDictionary<MethodInfo, ConcurrentBag<IEnumerable<Parameter>>>();

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
            return RegisterParameterFor(testCase.Body as MethodCallExpression);
        }


        public TestDependencyRegistry RegisterParameterFor<T, U>(Expression<Func<T, U>> testCase)
        {
            return RegisterParameterFor(testCase.Body as MethodCallExpression);
        }

        private TestDependencyRegistry RegisterParameterFor(MethodCallExpression expression)
        {
            var parametrizedTestCase = ExtractParametrizedTestCase(expression);
            var testCases = parametrizedTestCases.GetOrAdd(parametrizedTestCase.method,
                key => new ConcurrentBag<IEnumerable<Parameter>>());
            testCases.Add(parametrizedTestCase.parameters);
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

        internal IEnumerable<IEnumerable<Parameter>> GetParameterSetsFor(MethodInfo method)
        {
            return parametrizedTestCases.TryGetValue(method, out var parameters) ? parameters : null;
        }
    }
    
}
