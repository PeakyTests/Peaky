// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Pocket;

namespace Peaky
{
    public delegate void TestCaseSetup<in T>(T test, TestTarget target, TestDependencyRegistry dependencyRegistry);

    public class TestDependencyRegistry
    {
        private readonly ConcurrentDictionary<MethodInfo, ConcurrentDictionary<string, TestCase>> parameterizedTestCases = new ConcurrentDictionary<MethodInfo, ConcurrentDictionary<string, TestCase>>();

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

        public TestDependencyRegistry RegisterParameterFor<T>(Expression<Action<T>> testCase, TestCaseSetup<T> caseSetup = null)
        {
            return RegisterParameterFor(testCase.Body as MethodCallExpression, caseSetup);
        }

        public TestDependencyRegistry RegisterParameterFor<T, U>(Expression<Func<T, U>> testCase, TestCaseSetup<T> caseSetup = null)
        {
            return RegisterParameterFor(testCase.Body as MethodCallExpression, caseSetup);
        }

        private TestDependencyRegistry RegisterParameterFor(MethodCallExpression expression, Delegate caseSetup)
        {
            var parameterizedTestCase = ExtractParameterizedTestCase(expression, caseSetup);
            var testCases = parameterizedTestCases.GetOrAdd(parameterizedTestCase.method,
                key => new ConcurrentDictionary<string, TestCase>());
            testCases.TryAdd(parameterizedTestCase.testCase.Parameters.GetQueryString(), parameterizedTestCase.testCase);
            return this;
        }

        private static (MethodInfo method, TestCase testCase) ExtractParameterizedTestCase(MethodCallExpression expression, Delegate caseSetup)
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

            return (method, new TestCase( new ParameterSet( values ), caseSetup));
        }

        internal IEnumerable<ParameterSet> GetParameterSetsFor(MethodInfo method)
        {
            return parameterizedTestCases.TryGetValue(method, out var testCases) ? testCases.Select(e => e.Value.Parameters) : null;
        }

        public IEnumerable<TestCaseSetup<T>> GetTestCasesSetupFor<T>(MethodInfo method) where T : IPeakyTest
        {
            if (parameterizedTestCases.TryGetValue(method, out var testCases))
            {
                foreach (var caseSetup in testCases.Where(tc => tc.Value.CaseSetup != null).Select(tc => tc.Value.CaseSetup))
                {
                    yield return (TestCaseSetup<T>) caseSetup;
                }
            }
        }
    }
}
