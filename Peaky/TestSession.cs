// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Peaky;

public class TestSession
{
    private readonly IHttpContextAccessor httpContextAccessor;

    internal TestSession(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public string GetString(string key) =>
        httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(key, out var value)
            ? value
            : null;

    public void SetString(string key, string value) =>
        httpContextAccessor.HttpContext.Response.Cookies.Append(key, value);
}