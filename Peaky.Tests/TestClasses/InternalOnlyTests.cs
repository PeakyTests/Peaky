using System;

namespace Peaky.Tests.TestClasses
{
    public class InternalOnlyTests : IApplyToEnvironment
    {
        public dynamic internal_only_test() => "success!";

        public bool AppliesToEnvironment(string environment) => !environment.Equals("production", StringComparison.OrdinalIgnoreCase);
    }
}