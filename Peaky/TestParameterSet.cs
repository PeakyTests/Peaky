using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Peaky;

internal class TestParameterSet : IEnumerable<TestParameter>
{
    private readonly ImmutableArray<TestParameter> parameters;
    private readonly string queryString;

    public TestParameterSet(IEnumerable<TestParameter> parameters)
    {
        this.parameters = parameters.ToImmutableArray();
        queryString = ToUrlQueryString();
    }

    public IEnumerator<TestParameter> GetEnumerator()
    {
        foreach (var parameter in parameters)
        {
            yield return parameter;
        }
    }

    public string GetQueryString()
    {
        return queryString;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private string ToUrlQueryString()
    {
        return HttpRequestExtensions.GetQueryString(this);
    }
}