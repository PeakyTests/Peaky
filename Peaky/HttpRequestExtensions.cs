// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http;

namespace Peaky
{
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
}