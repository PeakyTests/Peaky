// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json;

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

        public TimeSpan Duration { get; private set; }

        public Exception Exception { get; private set; }

        public string Application { get; set; }

        public string TestName { get; set; }

        public string TestEnvironment { get; set; }

        public string Url { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] Tags { get; set; }

        public static TestResult Pass(
            object returnValue,
            TimeSpan duration,
            string application,
            string environment,
            string testName,
            string[] testTags,
            string testUrl)
        {

            if (string.IsNullOrWhiteSpace(application))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(application));
            }

            if (string.IsNullOrWhiteSpace(environment))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(environment));
            }

            if (string.IsNullOrWhiteSpace(testName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(testName));
            }

            if (string.IsNullOrWhiteSpace(testUrl))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(testUrl));
            }

            var testResult = new TestResult
            {
                ReturnValue = returnValue,
                Log = TraceBuffer.Current?.ToString(),
                Passed = true,
                Duration = duration,
                Application = application,
                TestEnvironment = environment,
                Tags = testTags,
                TestName = testName,
                Url = testUrl
            };

            return testResult;
        }

        public static TestResult Fail(
            Exception exception,
            TimeSpan duration, 
            string application,
            string environment,
            string testName, 
            string[] testTags, 
            string testUrl)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            if (string.IsNullOrWhiteSpace(application))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(application));
            }

            if (string.IsNullOrWhiteSpace(environment))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(environment));
            }

            if (string.IsNullOrWhiteSpace(testName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(testName));
            }

            if (string.IsNullOrWhiteSpace(testUrl))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(testUrl));
            }

            var testResult = new TestResult
            {
                Log = TraceBuffer.Current?.ToString(),
                Passed = false,
                Exception = exception,
                Duration = duration,
                Application = application,
                TestEnvironment = environment,
                Tags = testTags,
                TestName = testName,
                Url = testUrl
            };

            return testResult;
        }
    }
}
