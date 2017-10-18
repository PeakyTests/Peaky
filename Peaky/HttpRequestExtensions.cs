using Microsoft.AspNetCore.Http;
using Peaky;

internal static class HttpRequestExtensions
{
    internal static string GetLink(
       this HttpRequest request,
        TestTarget testTarget,
        TestDefinition testDefinition)
    {
        var scheme = request.Scheme;
        var host = request.Host;

        return $"{scheme}://{host}/tests/{testTarget.Environment}/{testTarget.Application}/{testDefinition.TestName}";
    }
}