// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Peaky;

public class TestResult
{
    private TestResult()
    {
    }

    public TestOutcome Outcome { get; private init; }

    public object ReturnValue { get; private set; }

    public bool Passed => Outcome == TestOutcome.Passed;

    public string Log { get; private set; }

    public TimeSpan Duration { get; private set; }

    public Exception Exception { get; private set; }

    public Test Test { get; private set; }

    internal static TestResult CreatePassedResult(
        object returnValue,
        TimeSpan duration,
        Test test)
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

    internal static TestResult CreateInconclusiveResult(
        TestInconclusiveException exception,
        TimeSpan duration,
        Test test)
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
            Outcome = TestOutcome.Inconclusive
        };

        return testResult;
    }

    internal static TestResult CreateTimeoutResult(
        TestTimeoutException exception,
        TimeSpan duration,
        Test test)
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

    internal static TestResult CreateFailedResult(
        Exception exception,
        TimeSpan duration, 
        Test test)
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