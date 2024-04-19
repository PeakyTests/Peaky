// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace Peaky;

internal static class HttpRequestExtensions
{
    internal static string GetLink(
        this HttpRequest request,
        TestTarget testTarget,
        TestDefinition testDefinition)
    {
        var scheme = testTarget.BaseAddress.Scheme switch
        {
            "https" => "https",
            _ => request.Scheme
        };

        var host = request.Host;

        return $"{scheme}://{host}/tests/{testTarget.Environment}/{testTarget.Application}/{testDefinition.TestName}";
    }

    internal static string GetLinkWithQuery(
        this HttpRequest request,
        TestTarget testTarget,
        TestDefinition testDefinition,
        IEnumerable<Parameter> parameters)
    {
        var query = GetQueryString(parameters);
        query = string.IsNullOrWhiteSpace(query) ? string.Empty : $"?{query}";
        return $"{request.GetLink(testTarget, testDefinition)}{query}";
    }

    internal static string GetLinkWithQuery(
        this HttpRequest request,
        TestTarget testTarget,
        TestDefinition testDefinition,
        ParameterSet parameters)
    {
        var query = parameters.GetQueryString();
        query = string.IsNullOrWhiteSpace(query) ? string.Empty : $"?{query}";
        return $"{request.GetLink(testTarget, testDefinition)}{query}";
    }

    internal static string GetQueryString(IEnumerable<Parameter> parameters)
    {
        return string.Join(
            "&",
            parameters
                .OrderBy(p => p.Name)
                .Where(p => p.DefaultValue != null)
                .Select(p => $"{p.Name}={HttpUtility.UrlEncode(p.DefaultValue.ToString())}"));
    }
}