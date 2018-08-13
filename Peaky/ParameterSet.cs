using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Peaky
{
    internal class ParameterSet : IEnumerable<Parameter>
    {
        private readonly ImmutableDictionary<string, object> parameters;
        private readonly string hash;

        public ParameterSet(IEnumerable<Parameter> parameters)
        {
            this.parameters = parameters.ToImmutableDictionary(e => e.Name, e => e.DefaultValue);
            hash = ToUrlQueryString();
        }

        public override bool Equals(object obj)
        {
            return obj is ParameterSet set &&
                   hash == set.hash;
        }

        public IEnumerator<Parameter> GetEnumerator()
        {
            foreach (var parameter in parameters)
            {
                yield return new Parameter(parameter.Key, parameter.Value);
            }
        }

        public override int GetHashCode()
        {
            return hash.GetHashCode();
        }

        public string GetQueryString()
        {
            return hash;
        }

        public override string ToString()
        {
            return hash;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private string ToUrlQueryString()
        {
            return string.Join(
                "&",
                parameters
                    .Where(p => p.Value != null)
                    .OrderBy(p => p.Key)
                    .Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value.ToString())}"));
        }
    }
}