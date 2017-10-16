using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace Peaky
{
    internal class AnonymousRouter : IRouter
    {
        private readonly Func<RouteContext, Task> routeAsync;
        private readonly Func<VirtualPathContext, VirtualPathData> getVirtualPath;

        public AnonymousRouter(
            Func<RouteContext, Task> routeAsync,
            Func<VirtualPathContext, VirtualPathData> getVirtualPath)
        {
            this.routeAsync = routeAsync ?? 
                              throw new ArgumentNullException(nameof(routeAsync));
            this.getVirtualPath = getVirtualPath ??
                                  throw new ArgumentNullException(nameof(getVirtualPath));
        }

        public async Task RouteAsync(RouteContext context) => await routeAsync(context);

        public  VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return getVirtualPath(context);
        }
    }
}