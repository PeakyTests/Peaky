using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;

namespace Peaky.Tests.TestClasses
{
    public enum ParameterizedTestEnum{
        ValueOne,
        ValueTwo,
    }

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

        public void I_do_stuff(string testCaseId, bool expectedResult)
        {
            var env = contextLookup[testCaseId];
            env.Value.Should().Be(expectedResult);
        }

        public bool I_do_stuff_and_return_bool(string testCaseId, bool expectedResult)
        {
            var context = contextLookup[testCaseId];
            context.Value.Should().Be(expectedResult);
            return context.Value;
        }

        public async Task I_do_stuff_and_return_task(string testCaseId, bool expectedResult)
        {
            await Task.Yield();
            var context = contextLookup[testCaseId];
            context.Value.Should().Be(expectedResult);
        }

        public async Task<bool> I_do_stuff_and_return_task_of_bool(string testCaseId, bool expectedResult)
        {
            await Task.Yield();
            var context = contextLookup[testCaseId];
            context.Value.Should().Be(expectedResult);
            return context.Value;
        }

        public void I_use_enum(ParameterizedTestEnum value)
        {

        }

        private class ParameterizedTestContext
        {
            public bool Value { get; set; }
        }

        public void RegisterTestCasesTo(TestDependencyRegistry registry)
        {
            registry.RegisterParameters(() => I_do_stuff("case1", true));

            registry.RegisterParameters(() => I_do_stuff("case2", false));

            registry.RegisterParameters(() => I_do_stuff("case3", true));

            registry.RegisterParameters(() => I_do_stuff("case4", false));

            registry.RegisterParameters(() => I_do_stuff("case5", true));

            registry.RegisterParameters(() => I_do_stuff_and_return_bool("case6", true));

            registry.RegisterParameters(() => I_do_stuff_and_return_bool("case7", false));

            registry.RegisterParameters(() => I_do_stuff_and_return_task("case8", false));

            registry.RegisterParameters(() => I_do_stuff_and_return_task_of_bool("case9", false));

            registry.RegisterParameters(() => I_use_enum(ParameterizedTestEnum.ValueOne));

            registry.RegisterParameters(() => I_use_enum(ParameterizedTestEnum.ValueTwo));
        }
    }
}