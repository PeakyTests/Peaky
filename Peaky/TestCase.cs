using System;

namespace Peaky
{
    internal class TestCase
    {
        public ParameterSet Parameters { get; }
        public Delegate CaseSetup { get; }

        public TestCase(ParameterSet parameters, Delegate caseSetup)
        {
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            CaseSetup = caseSetup;
        }
    }
}