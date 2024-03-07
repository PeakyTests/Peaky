using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Peaky;

internal class ParameterSet : IEnumerable<Parameter>
{
    private readonly ImmutableArray<Parameter> parameters;
    private readonly string queryString;

    public ParameterSet(IEnumerable<Parameter> parameters)
    {
        this.parameters = parameters.ToImmutableArray();
        queryString = ToUrlQueryString();
    }

    public IEnumerator<Parameter> GetEnumerator()
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