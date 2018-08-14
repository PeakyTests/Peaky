namespace Peaky.Tests.TestClasses
{
    public class TestDiscoveryResponse
    {
        public Test[] Tests { get; set; }

        public class Test
        {
            public string Application { get; set; }
            public string Environment { get; set; }
            public string Url { get; set; }
            public Parameter[] Parameters { get; set; }

            public class Parameter
            {
                public string Name { get; set; }
                public object DefaultValue { get; set; }
            }
        }
    }
}