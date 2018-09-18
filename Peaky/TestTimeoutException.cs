using System;

namespace Peaky
{
    public class TestTimeoutException : Exception
    {
        public TestTimeoutException()
        {
        }

        public TestTimeoutException(string message) : base(message)
        {
        }

        public TestTimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}