using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Peaky
{
    internal class ParameterSet : IEnumerable<Parameter>
    {
        private readonly ImmutableDictionary<string, object> parameters;
        private readonly string queryString;

        public ParameterSet(IEnumerable<Parameter> parameters)
        {
            this.parameters = parameters.ToImmutableDictionary(e => e.Name, e => e.DefaultValue);
            queryString = ToUrlQueryString();
        }

        public IEnumerator<Parameter> GetEnumerator()
        {
            foreach (var parameter in parameters)
            {
                yield return new Parameter(parameter.Key, parameter.Value);
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
}