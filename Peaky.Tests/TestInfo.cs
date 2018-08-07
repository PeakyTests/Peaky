using System;

namespace Peaky.Tests
{
    public class TestInfo
    {
        public string Application { get; set; }

        public string Name { get; set; }

        public string Environment { get; set; }

        public Uri Url { get; set; }
        public string[] Tags { get; set; }
    }
}