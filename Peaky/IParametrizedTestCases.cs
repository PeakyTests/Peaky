namespace Peaky
{
    public interface IParameterizedTestCases : IPeakyTest
    {
        void RegisterTestCasesTo(TestDependencyRegistry registry);
    }
}