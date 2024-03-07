using System;

namespace Peaky;

public class TestFailedException : Exception
{
    public TestFailedException()
    {
    }

    public TestFailedException(string message) : base(message)
    {
    }

    public TestFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}