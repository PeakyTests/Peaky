// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Peaky
{
    internal class TestResult
    {
        private TestResult()
        {
        }

        public TestOutcome Outcome { get; private set; }
        public object ReturnValue { get; private set; }

        public bool Passed => Outcome == TestOutcome.Passed;

        public string Log { get; private set; }

        public TimeSpan Duration { get; private set; }

        public Exception Exception { get; private set; }

        public TestInfo Test { get; private set; }

        public bool SupportsRetry { get; private set; }

        public static TestResult Pass(
            object returnValue,
            TimeSpan duration,
            TestInfo test)
        {
            var testResult = new TestResult
            {
                ReturnValue = returnValue,
                Log = TraceBuffer.Current?.ToString(),
                Duration = duration,
                Test = test,
                Outcome = TestOutcome.Passed
            };

            return testResult;
        }

        public static TestResult Inconclusive(
            TestInconclusiveException exception,
            TimeSpan duration,
            TestInfo test)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var testResult = new TestResult
            {
                Log = TraceBuffer.Current?.ToString(),
                Exception = exception,
                Duration = duration,
                Test = test,
                SupportsRetry = true,
                Outcome = TestOutcome.Inconclusive
            };

            return testResult;
        }

        public static TestResult Timeout(
            TestTimeoutException exception,
            TimeSpan duration,
            TestInfo test)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var testResult = new TestResult
            {
                Log = TraceBuffer.Current?.ToString(),
                Exception = exception,
                Duration = duration,
                Test = test,
                Outcome = TestOutcome.Timeout
            };

            return testResult;
        }

        public static TestResult Fail(
            Exception exception,
            TimeSpan duration, 
            TestInfo test)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }
 
            var testResult = new TestResult
            {
                Log = TraceBuffer.Current?.ToString(),
                Exception = exception,
                Duration = duration,
                Test = test,
                Outcome = TestOutcome.Failed
            };

            return testResult;
        }
    }
}
