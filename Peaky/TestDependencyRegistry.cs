// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Pocket;

namespace Peaky
{
    public class TestDependencyRegistry
    {
        private readonly ConcurrentDictionary<MethodInfo, ConcurrentDictionary<string, TestCase>> parameterizedTestCases =
            new ConcurrentDictionary<MethodInfo, ConcurrentDictionary<string, TestCase>>();

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

        public TestDependencyRegistry RegisterParametersFor(Expression<Action> testCase)
        {
            return RegisterParametersFor(testCase.Body as MethodCallExpression);
        }

        public TestDependencyRegistry RegisterParametersFor(Expression<Func<Task>> testCase)
        {
            return RegisterParametersFor(testCase.Body as MethodCallExpression);
        }

        private TestDependencyRegistry RegisterParametersFor(MethodCallExpression expression)
        {
            var parameterized = ExtractParameterizedTestCase(expression);

            var testCases = parameterizedTestCases.GetOrAdd(
                parameterized.method,
                key => new ConcurrentDictionary<string, TestCase>());

            testCases.TryAdd(
                parameterized.testCase.Parameters.GetQueryString(),
                parameterized.testCase);

            return this;
        }

        private static (MethodInfo method, TestCase testCase) ExtractParameterizedTestCase(MethodCallExpression expression)
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

            return (method, new TestCase(new ParameterSet(values)));
        }

        internal IReadOnlyCollection<ParameterSet> GetParameterSetsFor(MethodInfo method) =>
            parameterizedTestCases.TryGetValue(method, out var testCases)
                ? testCases.Select(e => e.Value.Parameters)
                           .ToArray()
                : Array.Empty<ParameterSet>();
    }
}
