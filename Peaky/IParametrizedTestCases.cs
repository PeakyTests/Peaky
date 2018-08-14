namespace Peaky
{
    public interface IParametrizedTestCases : IPeakyTest
    {
        void RegisterTestCasesTo(TestDependencyRegistry registry);
    }
}