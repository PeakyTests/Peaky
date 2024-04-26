using System;

namespace Peaky;

internal class TestCase
{
    public TestParameterSet Parameters { get; }

    public TestCase(TestParameterSet parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }
}