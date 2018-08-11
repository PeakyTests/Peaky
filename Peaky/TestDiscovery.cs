// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pocket;

namespace Peaky
{
    public class TestDefinitionRegistry : IEnumerable<TestDefinition>
    {
        private readonly Dictionary<string, TestDefinition> tests;

        public TestDefinitionRegistry(IEnumerable<Type> testTypes = null)
        {
            testTypes = testTypes ??
                        Discover.ConcreteTypes()
                                .DerivedFrom(typeof(IPeakyTest));

            tests = GetTestDefinitions(testTypes);
        }

        public TestDefinition Get(string testName)
        {
            return tests[testName];
        }

        public bool TryGet(string testName, out TestDefinition testDefinition) => 
            tests.TryGetValue(testName, out testDefinition);

        public static Dictionary<string, TestDefinition> GetTestDefinitions(
            IEnumerable<Type> concreteTestClasses)
        {
            var definitions =
                concreteTestClasses
                    .SelectMany(
                        t => t
                            .GetMethods(BindingFlags.Public |
                                        BindingFlags.Instance |
                                        BindingFlags.DeclaredOnly)
                            .Where(NotDefinedOn<IApplyToApplication>)
                            .Where(NotDefinedOn<IApplyToEnvironment>)
                            .Where(NotDefinedOn<IApplyToTarget>)
                            .Where(NotDefinedOn<IParametrizedTestCases>)
                            .Where(m => !m.IsSpecialName)
                            .Select(TestDefinition.Create));

            var dictionary = new Dictionary<string, TestDefinition>();
            var collisionCount = 0;

            foreach (var definition in definitions)
            {
                if (!dictionary.TryAdd(definition.TestName, definition))
                {
                    definition.TestName = $"{definition.TestName}__{++collisionCount}";
                    dictionary.Add(definition.TestName, definition);
                }
            }

            return dictionary;
        }

        public IEnumerator<TestDefinition> GetEnumerator() => tests.Values.GetEnumerator();

        private static bool NotDefinedOn<T>(MethodInfo method)
        {
            if (method.DeclaringType.GetInterface(typeof(T).Name) == null)
            {
                return true;
            }
            var map = method.DeclaringType.GetInterfaceMap(typeof(T));
            var found = map.TargetMethods.Contains(method);
            return !found;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
