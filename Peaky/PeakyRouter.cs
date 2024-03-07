// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Peaky;

internal abstract class PeakyRouter : IRouter
{
    protected PeakyRouter(string pathBase)
    {
        if (string.IsNullOrWhiteSpace(pathBase))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(pathBase));
        }

        PathBase = new PathString(pathBase);
    }

    public PathString PathBase { get; }

    async Task IRouter.RouteAsync(RouteContext context)
    {
        if (context.Handler != null)
        {
            return;
        }

        if (RouteMatches(context))
        {
            await RouteAsync(context);
        }
    }

    public abstract Task RouteAsync(RouteContext context);

    public virtual VirtualPathData GetVirtualPath(VirtualPathContext context) => null;

    protected bool RouteMatches(RouteContext routeContext) => routeContext.HttpContext.Request.Path.StartsWithSegments(PathBase, StringComparison.OrdinalIgnoreCase);
}