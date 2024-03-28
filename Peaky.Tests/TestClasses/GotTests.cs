using System;
using System.Threading.Tasks;
using Its.Recipes;

namespace Peaky.Tests.TestClasses;

public class GotTests : IPeakyTest
{
    public string SomeProperty { get; set; }

    public string passing_test_returns_object()
    {
        return "success!";
    }

    public bool passing_test_returns_struct()
    {
        return true;
    }

    public dynamic failing_test()
    {
        throw new TestFailedException("oops!");
    }

    public dynamic throwing_test()
    {
        throw new Exception("oops!");
    }

    public dynamic inconclusive_test()
    {
        throw new TestInconclusiveException("service not available, retry later");
    }

    public dynamic timingout_test()
    {
        throw new TestTimeoutException("timing out!!!");
    }

    public void passing_void_test()
    {
    }

    public void void_test_with_optional_parameters(string foo = "bar")
    {
    }

    public void test_with_non_optional_parameters(string foo)
    {
    }

    public string string_returning_test_with_optional_parameters(string foo = "bar", int count = 1)
    {
        return $"{foo} - {count}";
    }

    public void failing_void_test()
    {
        throw new Exception("oops!");
    }

    public async Task passing_void_async_test()
    {
        await Task.Yield();
    }

    public async Task failing_void_async_test()
    {
        await Task.Yield();

        throw new Exception("oops!");
    }

    public static dynamic not_a_test()
    {
        return null;
    }

    private dynamic also_not_a_test()
    {
        return null;
    }

    public void no_more_than_10_percent_of_calls_have_failed()
    {
        Telemetry[] telemetry =
        {
            new Telemetry
            {
                ElapsedMilliseconds = Any.Int(),
                OperationName = Any.CamelCaseName(),
                Succeeded = false,
                UserIdentifier = Any.Email(),
                Properties = { { "StatusCode", "500" } }
            },
            new Telemetry
            {
                ElapsedMilliseconds = Any.Int(),
                OperationName = Any.CamelCaseName(),
                Succeeded = true,
                UserIdentifier = Any.Email(),
                Properties = { { "StatusCode", "200" } }
            }
        };

        telemetry.PercentageOf(t => !t.Succeeded).Should().BeLessThanOrEqualTo(10.Percent());
    }

    public void telemetry_without_failures()
    {
        Telemetry[] telemetry =
        {
            new Telemetry
            {
                ElapsedMilliseconds = Any.Int(),
                OperationName = Any.CamelCaseName(),
                Succeeded = true,
                UserIdentifier = Any.Email(),
                Properties = { { "StatusCode", "200" } }
            },
            new Telemetry
            {
                ElapsedMilliseconds = Any.Int(),
                OperationName = Any.CamelCaseName(),
                Succeeded = true,
                UserIdentifier = Any.Email(),
                Properties = { { "StatusCode", "200" } }
            }
        };

        telemetry.PercentageOf(t => t.Succeeded).Should().BeEqualTo(100.Percent());
    }
}