using System.Net.Http;

namespace Peaky.SampleWebApplication
{
    public class ParameterizedTests : IApplyToApplication,
        IApplyToEnvironment,
        IHaveTags,
        IParameterizedTestCases
    {
        private readonly HttpClient httpClient;

        public ParameterizedTests(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public bool AppliesToApplication(string application)
        {
            return application == "parameterized";
        }

        public bool AppliesToEnvironment(string environment)
        {
            return environment == "test";
        }

        public string[] Tags => new[]
        {
            "NonSideEffecting"
        };

        public void parameterised_test(int value)
        {

        }

        public void parameterised_test_with_enum(TestParameterEnum value)
        {

        }

        public void RegisterTestCasesTo(TestDependencyRegistry registry)
        {
            registry.RegisterParametersFor(() => parameterised_test(1));
            registry.RegisterParametersFor(() => parameterised_test(2));
            registry.RegisterParametersFor(() => parameterised_test(3));
            registry.RegisterParametersFor(() => parameterised_test(4));

            registry.RegisterParametersFor(() => parameterised_test_with_enum(TestParameterEnum.First));
            registry.RegisterParametersFor(() => parameterised_test_with_enum(TestParameterEnum.Second));
        }
    }

    public enum TestParameterEnum
    {
        First,
        Second
    }
}