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

        public object ReturnValue { get; private set; }

        public bool Passed { get; private set; }

        public string Log { get; private set; }

        public Exception Exception { get; private set; }

        public static TestResult Pass(object returnValue)
        {
            var testResult = new TestResult
            {
                ReturnValue = returnValue,
                Log = TraceBuffer.Current?.ToString(),
                Passed = true
            };

            return testResult;
        }

        public static TestResult Fail(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var testResult = new TestResult
            {
                Log = TraceBuffer.Current?.ToString(),
                Passed = false,
                Exception = exception
            };

            return testResult;
        }
    }
}
