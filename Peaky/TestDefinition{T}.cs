// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pocket;
using static Pocket.Logger<Peaky.TestDefinition>;

namespace Peaky
{
    internal class TestDefinition<T> : TestDefinition
        where T : IPeakyTest
    {
        private readonly Func<T, dynamic> defaultExecuteTestMethod;
       
        private readonly ConcurrentDictionary<TestTarget, DetailsForTarget> applicabilityCache = new ConcurrentDictionary<TestTarget, DetailsForTarget>();

        internal TestDefinition(MethodInfo methodInfo)
        {
            TestMethod = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));

            TestName = methodInfo.Name;

            defaultExecuteTestMethod = BuildTestMethodExpression(
                methodInfo,
                methodInfo.GetParameters()
                          .Select(p => Expression.Constant(p.GetDefaultValue(), p.ParameterType)));
        }


        public override bool AppliesTo(TestTarget target) =>
            applicabilityCache.GetOrAdd(target, t => new DetailsForTarget(t))
                              .IsApplicable;

        public override string[] Tags =>
            applicabilityCache.FirstOrDefault()
                              .Value
                              ?.Tags
            ??
            Array.Empty<string>();

        internal override async Task<object> Run(HttpContext context, Func<Type, object> resolve, TestTarget target)
        {
            var executeTestMethod = defaultExecuteTestMethod;
            var methodParameters = TestMethod.GetParameters();

            var queryParameters = context.Request.Query;

            if (queryParameters.Keys.Any(p => methodParameters.Select(pp => pp.Name).Contains(p)))
            {
                executeTestMethod = BuildTestMethodExpression(
                    TestMethod,
                    methodParameters
                        .Select(p =>
                        {
                            var value = queryParameters[p.Name].FirstOrDefault() ??
                                        p.DefaultValue;
                            try
                            {
                                var castedValue = Convert.ChangeType(value, p.ParameterType);
                                return Expression.Constant(castedValue);
                            }
                            catch (FormatException e)
                            {
                                throw new ParameterFormatException(p.Name, p.ParameterType, e);
                            }
                        }));
            }

            var testClassInstance = (T) resolve(typeof(T));
            switch (testClassInstance)
            {
                case IParametrizedTestCases parametrizedTest:
                    parametrizedTest.RegisterTestCasesTo(target.DependencyRegistry);
                    PerformTestCaseSetup(testClassInstance, target);
                    break;
            }
            return executeTestMethod(testClassInstance);
        }

        private void PerformTestCaseSetup(T testClassInstance, TestTarget target)
        {
            var setupCalls = target.DependencyRegistry.GetTestCasesSetupFor<T>(TestMethod);
            foreach (var setupCall in setupCalls)
            {
                setupCall(testClassInstance, target, target.DependencyRegistry);
            }
        }

        public override string ToString() =>
            $"{base.ToString()} ({TestMethod.DeclaringType}.{TestMethod.Name})";

        private static Func<T, dynamic> BuildTestMethodExpression(MethodInfo methodInfo, IEnumerable<ConstantExpression> parameters)
        {
            var test = Expression.Parameter(typeof(T), "test");

            if (methodInfo.ReturnType != typeof(void))
            {
                if (methodInfo.ReturnType.IsClass)
                {
                    return Expression.Lambda<Func<T, dynamic>>(
                        Expression.Call(
                            test,
                            methodInfo,
                            parameters),
                        test).Compile();
                }

                dynamic testMethod = Expression.Lambda(typeof(Func<,>)
                                                           .MakeGenericType(typeof(T), methodInfo.ReturnType),
                                                       Expression.Call(test,
                                                                       methodInfo,
                                                                       parameters),
                                                       test).Compile();

                return testClassInstance => testMethod(testClassInstance);
            }

            var voidRunMethod = Expression.Lambda<Action<T>>(Expression.Call(test,
                                                                             methodInfo,
                                                                             parameters),
                                                             test).Compile();

            return testClassInstance =>
            {
                voidRunMethod(testClassInstance);
                return new object();
            };
        }

        private class DetailsForTarget
        {
            private readonly TestTarget target;

            public DetailsForTarget(TestTarget target)
            {
                this.target = target ?? throw new ArgumentNullException(nameof(target));

                try
                {
                    Initialize();
                }
                catch (Exception exception)
                {
                    Log.Warning("Dependency resolution error while trying to instantiate {type}", exception, typeof(T));

                    // return true which will allow the test execution error to be displayed 
                    IsApplicable = true;
                }
            }

            private void Initialize()
            {
                var testClassInstance = target.DependencyRegistry.Container.Resolve(typeof(T));

                if (target.Environment != null &&
                    testClassInstance is IApplyToEnvironment e &&
                    !e.AppliesToEnvironment(target.Environment))
                {
                    IsApplicable = false;
                }
                else if (target.Application != null &&
                         testClassInstance is IApplyToApplication a &&
                         !a.AppliesToApplication(target.Application))
                {
                    IsApplicable = false;
                }
                else if (testClassInstance is IApplyToTarget t &&
                         !t.AppliesToTarget(target))
                {
                    IsApplicable = false;
                }
                else
                {
                    IsApplicable = true;
                }

                if (testClassInstance is IHaveTags hasTags)
                {
                    Tags = hasTags.Tags;
                }
            }

            public string[] Tags { get; private set; }

            public bool IsApplicable { get; private set; }
        }
    }
}
