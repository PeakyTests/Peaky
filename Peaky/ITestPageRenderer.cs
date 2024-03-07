// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Peaky;

public interface ITestPageRenderer
{
    Task Render(HttpContext httpContext);
}