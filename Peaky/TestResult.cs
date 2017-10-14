using System;

namespace Peaky
{
    internal class TestResult
    {
        private TestResult()
        {
        }

        public object ReturnValue { get; private set; }

        public bool Passed { get; private set; }

        public string Log { get; private set; }

        public Exception Exception { get; private set; }

        public static TestResult Pass(object returnValue, string log = "")
        {
            return new TestResult
            {
                ReturnValue = returnValue,
                Log = log,
                Passed = true
            };
        }

        public static TestResult Fail(Exception exception, string log = "")
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            return new TestResult
            {
                Log = log,
                Passed = false,
                Exception = exception
            };
        }
    }
}
