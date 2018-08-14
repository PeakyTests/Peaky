using System;
using System.Collections.Generic;
using FluentAssertions;

namespace Peaky.Tests.TestClasses
{
    public class ParametrizedTest : IApplyToApplication, IParametrizedTestCases
    {
        private readonly Dictionary<string, EnvironmentStuff> _environmentLookup;
        public bool AppliesToApplication(string application) => application.Equals("parametrized", StringComparison.OrdinalIgnoreCase);

        public ParametrizedTest()
        {
            _environmentLookup = new Dictionary<string, EnvironmentStuff>
            {
                { "case1", new EnvironmentStuff{Value = true} },
                { "case2", new EnvironmentStuff {Value = false} },
                { "case3", new EnvironmentStuff {Value = true} },
                { "case4", new EnvironmentStuff{Value = false} }
            };
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

        private class EnvironmentStuff
        {
            public bool Value { get; set; }
        }

        public void RegisterTestCasesTo(TestDependencyRegistry registry)
        {
            registry.RegisterParameterFor<ParametrizedTest>(testClass => testClass.I_do_stuff("case1", true));
            registry.RegisterParameterFor<ParametrizedTest>(testClass => testClass.I_do_stuff("case2", false));
            registry.RegisterParameterFor<ParametrizedTest>(testClass => testClass.I_do_stuff("case3", true));
            registry.RegisterParameterFor<ParametrizedTest>(testClass => testClass.I_do_stuff("case4", false));

            registry.RegisterParameterFor<ParametrizedTest>(testClass => testClass.I_do_stuff("case5", true),

                (test, target, dependencyRegistry) => test._environmentLookup["case5"] = new EnvironmentStuff { Value = true });

            registry.RegisterParameterFor<ParametrizedTest, bool>(testClass => testClass.I_do_stuff_and_return("case1", true));
            registry.RegisterParameterFor<ParametrizedTest, bool>(testClass => testClass.I_do_stuff_and_return("case2", false));
        }
    }
}