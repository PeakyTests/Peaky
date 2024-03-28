namespace Peaky.Tests.TestClasses;

public class AppleTests : IHaveTags
{
    public string[] Tags => new[]
    {
        "apple", "fruit"
    };

    public dynamic honeycrisp() => "Yum!";
}