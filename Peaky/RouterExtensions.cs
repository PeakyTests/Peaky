using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;

namespace Peaky
{
    internal static class RouterExtensions
    {
        public static IRouter Accept(this IRouter router, MediaTypeHeaderValue mediaType)
        {
            if (router == null)
            {
                throw new ArgumentNullException(nameof(router));
            }

            return new AnonymousRouter(
                routeAsync: async context =>
                {
                    var requestHeader = context.HttpContext.Request.Headers["Accept"];

                    if (!requestHeader.Select(h => MediaTypeHeaderValue.Parse(h)).Any(h => h.Equals(mediaType)))
                    {
                        return;
                    }

                    await router.RouteAsync(context);
                },
                getVirtualPath: router.GetVirtualPath);
        }

        public static IRouter AllowVerbs(this IRouter router, params string[] verbs)
        {
            if (router == null)
            {
                throw new ArgumentNullException(nameof(router));
            }

            var allowedVerbs = new HashSet<string>(verbs, StringComparer.OrdinalIgnoreCase);

            return new AnonymousRouter(
                routeAsync: async context =>
                {
                    if (!allowedVerbs.Contains(context.HttpContext.Request.Method))
                    {
                        context.Handler = async httpContext => httpContext.Response.StatusCode = 405;
                    }

                    await router.RouteAsync(context);
                },
                getVirtualPath: router.GetVirtualPath);
        }
    }
}
