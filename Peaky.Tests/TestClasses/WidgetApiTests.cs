using System;

namespace Peaky.Tests.TestClasses;

public class WidgetApiTests : IApplyToApplication
{
    public dynamic widgetapi_only_test() => "success!";

    public bool AppliesToApplication(string application) => application.Equals("widgetapi", StringComparison.OrdinalIgnoreCase);
}