using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Pocket;
using static Pocket.Logger<Peaky.SensorRouter>;

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
            var path = context.HttpContext.Request.Path;

            var testRootPath = "/sensors";

            if (!path.StartsWithSegments(new PathString(testRootPath)))
            {
                return;
            }

            var segments = path.Value
                               .Substring(testRootPath.Length)
                               .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            switch (segments.Length)
            {
                case 0:
                    ListSensors(context);
                    break;

                case 1:
                    ReadSensor(segments[0], context);
                    break;
            }
        }

        private void ReadSensor(string sensorName, RouteContext context)
        {
            using (Log.OnEnterAndExit())
            {
                if (!sensors.TryGet(sensorName, out var sensor))
                {
                    return;
                }

                context.Handler = async httpContext =>
                {
                    var reading = sensor.Read();

                    var json = JsonConvert.SerializeObject(reading);

                    await httpContext.Response.WriteAsync(json);
                };
            }
        }

        private void ListSensors(RouteContext context)
        {
            using (Log.OnEnterAndExit())
            {
            }
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }
    }
}
