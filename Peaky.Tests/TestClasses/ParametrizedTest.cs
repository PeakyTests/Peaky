using System;
using System.Collections.Generic;
using FluentAssertions;

namespace Peaky.Tests.TestClasses
{
    public class ParameterizedTest : IApplyToApplication, IParameterizedTestCases
    {
        private readonly Dictionary<string, TestEnvironment> _environmentLookup = new Dictionary<string, TestEnvironment>();
        public bool AppliesToApplication(string application) => application.Equals("Parameterized", StringComparison.OrdinalIgnoreCase);

        public ParameterizedTest()
        {
           
        }

        public void TestCase_should_meet_expectation(string testCaseId, bool extectedResult)
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
            registry.RegisterParameterFor<ParameterizedTest>(testClass => testClass.TestCase_should_meet_expectation("case1", true),
                (test, target, dependencyRegistry) => test._environmentLookup["case1"] = new TestEnvironment { Value = true });

            registry.RegisterParameterFor<ParameterizedTest>(testClass => testClass.TestCase_should_meet_expectation("case2", false),
                (test, target, dependencyRegistry) => test._environmentLookup["case2"] = new TestEnvironment { Value = false });

            registry.RegisterParameterFor<ParameterizedTest>(testClass => testClass.TestCase_should_meet_expectation("case3", true),
                (test, target, dependencyRegistry) => test._environmentLookup["case3"] = new TestEnvironment { Value = true });

            registry.RegisterParameterFor<ParameterizedTest>(testClass => testClass.TestCase_should_meet_expectation("case4", false),
                (test, target, dependencyRegistry) => test._environmentLookup["case4"] = new TestEnvironment { Value = false });

            registry.RegisterParameterFor<ParameterizedTest>(testClass => testClass.TestCase_should_meet_expectation("case5", true),
                (test, target, dependencyRegistry) => test._environmentLookup["case5"] = new TestEnvironment { Value = true });

            registry.RegisterParameterFor<ParameterizedTest, bool>(testClass => testClass.I_do_stuff_and_return("case6", true),
                (test, target, dependencyRegistry) => test._environmentLookup["case6"] = new TestEnvironment { Value = true });

            registry.RegisterParameterFor<ParameterizedTest, bool>(testClass => testClass.I_do_stuff_and_return("case7", false),
                (test, target, dependencyRegistry) => test._environmentLookup["case7"] = new TestEnvironment { Value = false });
        }
    }
}