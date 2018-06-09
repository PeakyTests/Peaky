using System;

namespace Peaky.Tests
{
    public class TestResult
    {
        public object ReturnValue { get; set; }

        public bool Passed { get; set; }

        public string Log { get; set; }

        public object Exception { get; set; }

        public TimeSpan Duration { get; set; }
    }
}
