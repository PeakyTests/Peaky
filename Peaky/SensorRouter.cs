using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Peaky
{
    public class SensorRouter : IRouter
    {
        private readonly SensorRegistry sensors;

        public SensorRouter(SensorRegistry sensors)
        {
            this.sensors = sensors ??
                           throw new ArgumentNullException(nameof(sensors));
        }

        public async Task RouteAsync(RouteContext context)
        {
            if (!context.HttpContext.Request.Path.StartsWithSegments(new PathString("/sensors")))
            {
                return;
            }
            context.Handler = async httpContext =>
            {
                //                httpContext.Response.WriteAsync(sensor.Read());
            };
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }
    }
}
