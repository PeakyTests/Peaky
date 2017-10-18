using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Peaky
{
    internal static class RouterExtensions
    {
        public static PeakyRouter Accept(
            this PeakyRouter router,
            params string[] mediaTypes)
        {
            if (router == null)
            {
                throw new ArgumentNullException(nameof(router));
            }

            var acceptedMediaTypes = mediaTypes.ToMediaTypeCollection();

            return new AnonymousRouter(
                routeAsync: async context =>
                {
                    var acceptHeader = context.HttpContext.Request.Headers[HeaderNames.Accept];

                    if (acceptHeader.IsSubsetOfAny(acceptedMediaTypes))
                    {
                        await router.RouteAsync(context);
                    }
                },
                pathBase: router.PathBase);
        }

        private static MediaTypeCollection ToMediaTypeCollection(this string[] mediaTypes)
        {
            var acceptedMediaTypes = new MediaTypeCollection();
            foreach (var arg in mediaTypes)
            {
                acceptedMediaTypes.Add(arg);
            }
            return acceptedMediaTypes;
        }

        private static bool IsSubsetOfAny(
            this StringValues requestedMediaType,
            MediaTypeCollection mediaTypes)
        {
            if (requestedMediaType == default(StringValues))
            {
                return false;
            }

            var requested = new MediaType(requestedMediaType);

            return mediaTypes.Any(mediaType => requested.IsSubsetOf(new MediaType(mediaType)));
        }

        public static PeakyRouter AllowVerbs(this PeakyRouter router, params string[] verbs)
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
                        context.Handler = async httpContext =>
                        {
                            httpContext.Response.StatusCode = 405;
                        };
                    }

                    await router.RouteAsync(context);
                },
                pathBase: router.PathBase);
        }
    }
}
