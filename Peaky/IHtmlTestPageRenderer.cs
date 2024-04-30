// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Peaky;

public interface IHtmlTestPageRenderer
{
    Task RenderTestResultAsync(TestResult result, HttpContext httpContext);

    Task RenderTestListAsync(IReadOnlyList<Test> tests, HttpContext httpContext);
}