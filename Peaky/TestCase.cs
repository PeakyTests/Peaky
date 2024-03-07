using System;

namespace Peaky;

internal class TestCase
{
    public ParameterSet Parameters { get; }

    public TestCase(ParameterSet parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }
}