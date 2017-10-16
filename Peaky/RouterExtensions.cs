using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

namespace Peaky
{
    internal static class RouterExtensions
    {
        public static IRouter AllowVerbs(this IRouter router, params string[] verbs)
        {
            if (router == null)
            {
                throw new ArgumentNullException(nameof(router));
            }


            var allowedVerbs = new HashSet<string>(verbs, StringComparer.OrdinalIgnoreCase);

            return new AnonymousRouter(
                routeAsync: context =>
                {
                    if (!allowedVerbs.Contains(context.HttpContext.Request.Method))
                    {
                        context.Handler = async httpContext =>
                        {
                            httpContext.Response.StatusCode = 405;
                        };
                    }

                    return router.RouteAsync(context);
                },
                getVirtualPath: router.GetVirtualPath);
        }
    }
}