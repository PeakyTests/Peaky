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
        where T : class, IPeakyTest
    {
        private readonly Func<T, dynamic> defaultExecuteTestMethod;
        private readonly MethodInfo methodInfo;
        private readonly ConcurrentDictionary<TestTarget, ForTarget> applicabilityCache = new ConcurrentDictionary<TestTarget, ForTarget>();

        internal TestDefinition(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            this.methodInfo = methodInfo;

            TestName = methodInfo.Name;

            defaultExecuteTestMethod = BuildTestMethodExpression(
                methodInfo,
                methodInfo.GetParameters()
                          .Select(p => Expression.Constant(p.DefaultValue)));
        }

        public override bool AppliesTo(TestTarget target) =>
            applicabilityCache.GetOrAdd(target, t => new ForTarget(t))
                              .IsApplicable;

        public override string[] Tags =>
            applicabilityCache.FirstOrDefault()
                              .Value
                              ?.Tags
            ??
            Array.Empty<string>();

        internal override async Task<object> Run(HttpContext context, Func<Type, object> resolve)
        {
            var executeTestMethod = defaultExecuteTestMethod;
            var methodParameters = methodInfo.GetParameters();

            var queryParameters = context.Request.Query;

            if (queryParameters.Keys.Any(p => methodParameters.Select(pp => pp.Name).Contains(p)))
            {
                executeTestMethod = BuildTestMethodExpression(
                    methodInfo,
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

            return executeTestMethod(testClassInstance);
        }

        public override string ToString() =>
            $"{base.ToString()} ({methodInfo.DeclaringType}.{methodInfo.Name})";

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

        private class ForTarget
        {
            private readonly TestTarget target;

            public ForTarget(TestTarget target)
            {
                this.target = target ?? throw new ArgumentNullException(nameof(target));

                // TODO: (AppliesTo) cache the result per target
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
                var testClassInstance = target.ResolveDependency(typeof(T));

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
