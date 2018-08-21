using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;

namespace Peaky.Tests.TestClasses
{
    public class ParameterizedTest : IApplyToApplication, IParameterizedTestCases
    {
        private readonly Dictionary<string, ParameterizedTestContext> contextLookup = new Dictionary<string, ParameterizedTestContext>();
        public bool AppliesToApplication(string application) => application.Equals("Parameterized", StringComparison.OrdinalIgnoreCase);

        public ParameterizedTest()
        {
                contextLookup["case1"] = new ParameterizedTestContext { Value = true };
                contextLookup["case2"] = new ParameterizedTestContext { Value = false };
                contextLookup["case3"] = new ParameterizedTestContext { Value = true };
                contextLookup["case4"] = new ParameterizedTestContext { Value = false };
                contextLookup["case5"] = new ParameterizedTestContext { Value = true };
                contextLookup["case6"] = new ParameterizedTestContext { Value = true };
                contextLookup["case7"] = new ParameterizedTestContext { Value = false };
                contextLookup["case8"] = new ParameterizedTestContext { Value = false };
                contextLookup["case9"] = new ParameterizedTestContext { Value = false };
        }

        public void TestCase_should_meet_expectation(string testCaseId, bool extectedResult)
        {
            var env = contextLookup[testCaseId];
            env.Value.Should().Be(extectedResult);
        }

        public bool I_do_stuff_and_return_bool(string testCaseId, bool extectedResult)
        {
            var context = contextLookup[testCaseId];
            context.Value.Should().Be(extectedResult);
            return context.Value;
        }

        public async Task I_do_stuff_and_return_task(string testCaseId, bool extectedResult)
        {
            await Task.Yield();
            var context = contextLookup[testCaseId];
            context.Value.Should().Be(extectedResult);
        }

        public async Task<bool> I_do_stuff_and_return_task_of_bool(string testCaseId, bool extectedResult)
        {
            await Task.Yield();
            var context = contextLookup[testCaseId];
            context.Value.Should().Be(extectedResult);
            return context.Value;
        }

        private class ParameterizedTestContext
        {
            public bool Value { get; set; }
        }

        public void RegisterTestCasesTo(TestDependencyRegistry registry)
        {
            registry.RegisterParametersFor(() => TestCase_should_meet_expectation("case1", true));

            registry.RegisterParametersFor(() => TestCase_should_meet_expectation("case2", false));

            registry.RegisterParametersFor(() => TestCase_should_meet_expectation("case3", true));

            registry.RegisterParametersFor(() => TestCase_should_meet_expectation("case4", false));

            registry.RegisterParametersFor(() => TestCase_should_meet_expectation("case5", true));

            registry.RegisterParametersFor(() => I_do_stuff_and_return_bool("case6", true));

            registry.RegisterParametersFor(() => I_do_stuff_and_return_bool("case7", false));

            registry.RegisterParametersFor(() => I_do_stuff_and_return_task("case8", false));

            registry.RegisterParametersFor(() => I_do_stuff_and_return_task_of_bool("case9", false));
        }
    }
}