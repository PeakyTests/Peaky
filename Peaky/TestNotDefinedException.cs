using System;

namespace Peaky
{
    internal class TestNotDefinedException : Exception
    {
        public TestNotDefinedException(string message) : base(message)
        {
        }
    }
}