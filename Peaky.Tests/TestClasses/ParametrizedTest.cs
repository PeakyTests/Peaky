using System;
using System.Collections.Generic;
using FluentAssertions;

namespace Peaky.Tests.TestClasses
{
    public class ParametrizedTest : IApplyToApplication, IParametrizedTestCases
    {
        private readonly Dictionary<string, TestEnvironment> _environmentLookup = new Dictionary<string, TestEnvironment>();
        public bool AppliesToApplication(string application) => application.Equals("parametrized", StringComparison.OrdinalIgnoreCase);

        public ParametrizedTest()
        {
           
        }

        public void I_do_stuff(string testCaseId, bool extectedResult)
        {
            var env = _environmentLookup[testCaseId];
            env.Value.Should().Be(extectedResult);
        }

        public bool I_do_stuff_and_return(string testCaseId, bool extectedResult)
        {
            var env = _environmentLookup[testCaseId];
            env.Value.Should().Be(extectedResult);
            return env.Value;
        }

        private class TestEnvironment
        {
            public bool Value { get; set; }
        }

        public void RegisterTestCasesTo(TestDependencyRegistry registry)
        {
            registry.RegisterParameterFor<ParametrizedTest>(testClass => testClass.I_do_stuff("case1", true),
                (test, target, dependencyRegistry) => test._environmentLookup["case1"] = new TestEnvironment { Value = true });

            registry.RegisterParameterFor<ParametrizedTest>(testClass => testClass.I_do_stuff("case2", false),
                (test, target, dependencyRegistry) => test._environmentLookup["case2"] = new TestEnvironment { Value = false });

            registry.RegisterParameterFor<ParametrizedTest>(testClass => testClass.I_do_stuff("case3", true),
                (test, target, dependencyRegistry) => test._environmentLookup["case3"] = new TestEnvironment { Value = true });

            registry.RegisterParameterFor<ParametrizedTest>(testClass => testClass.I_do_stuff("case4", false),
                (test, target, dependencyRegistry) => test._environmentLookup["case4"] = new TestEnvironment { Value = false });

            registry.RegisterParameterFor<ParametrizedTest>(testClass => testClass.I_do_stuff("case5", true),
                (test, target, dependencyRegistry) => test._environmentLookup["case5"] = new TestEnvironment { Value = true });

            registry.RegisterParameterFor<ParametrizedTest, bool>(testClass => testClass.I_do_stuff_and_return("case6", true),
                (test, target, dependencyRegistry) => test._environmentLookup["case6"] = new TestEnvironment { Value = true });

            registry.RegisterParameterFor<ParametrizedTest, bool>(testClass => testClass.I_do_stuff_and_return("case7", false),
                (test, target, dependencyRegistry) => test._environmentLookup["case7"] = new TestEnvironment { Value = false });
        }
    }
}