using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace Peaky
{
    internal class AnonymousRouter : PeakyRouter
    {
        private readonly Func<RouteContext, Task> routeAsync;

        public AnonymousRouter(
            Func<RouteContext, Task> routeAsync,
            string pathBase) : base(pathBase)
        {
            this.routeAsync = routeAsync ??
                              throw new ArgumentNullException(nameof(routeAsync));
        }

        public override async Task RouteAsync(RouteContext context) => await routeAsync(context);
    }
}
