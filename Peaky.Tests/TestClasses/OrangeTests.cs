namespace Peaky.Tests.TestClasses;

public class OrangeTests : IHaveTags
{
    public string[] Tags => new[]
    {
        "orange", "fruit"
    };

    public dynamic tangerine() => "Oooh!";
}