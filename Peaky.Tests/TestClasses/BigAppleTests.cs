namespace Peaky.Tests.TestClasses
{
    public class BigAppleTests : IHaveTags
    {
        public string[] Tags => new[]
        {
            "Brooklyn", "Queens"
        };

        public dynamic manhattan() => "Empire State";
    }
}