using System;
using System.Collections.Generic;
using Its.Recipes;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pocket;
using static Pocket.Logger<Peaky.SensorRouter>;

namespace Peaky
{
    public class SensorRouter : IRouter
    {
        private readonly AuthorizeSensors authorizeSensors;

        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Error = (sender, args) =>
            {
                args.ErrorContext.Handled = true;
            }
        };

        private readonly SensorRegistry sensors;

        public SensorRouter(SensorRegistry sensors, AuthorizeSensors authorizeSensors)
        {
            this.authorizeSensors = authorizeSensors ??
                                    throw new ArgumentNullException(nameof(authorizeSensors));
            this.sensors = sensors ??
                           throw new ArgumentNullException(nameof(sensors));
        }

        public async Task RouteAsync(RouteContext context)
        {
            authorizeSensors(context);

            if (context.Handler != null)
            {
                return;
            }

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
                    var reading = await sensor.Read();

                    if (reading.Value is Task valueTask)
                    {
                        await valueTask;

                        if (valueTask.GetType().GetGenericArguments().First().IsVisible)
                        {
                            reading.Value = ((dynamic) valueTask).Result;
                        }
                        else
                        {
                            // this is required to work around the fact that internal types cause dynamic calls to Result to fail. JSON.NET however will happily serialize them, at which point we can retrieve the Result property.
                            var serialized = JsonConvert.SerializeObject(reading, serializerSettings);
                            Log.Trace(serialized);
                            reading.Value = JsonConvert.DeserializeObject<dynamic>(serialized).Result;
                        }
                    }

                    // add a self link
                    var localPath = httpContext.Request.Path.Value;
                    reading
                        .IfTypeIs<IDictionary<string, object>>()
                        .ThenDo(d => d["_links"] = new { self = localPath })
                        .ElseDo(() =>
                        {
                            var json = JsonConvert.SerializeObject(reading, serializerSettings);

                            if (!json.Contains("{"))
                            {
                                json = @"{""value"":" + json + @"}";
                            }

                            var jtoken = JsonConvert.DeserializeObject<JToken>(json);

                            jtoken.IfTypeIs<JObject>()
                                  .ThenDo(o => o.Add("_links", new JObject(new JProperty("self", localPath))));

                            reading = new SensorResult
                            {
                                SensorName = reading.SensorName,
                                Value = jtoken
                            };
                        });

                    var readingJson = JsonConvert.SerializeObject(reading, serializerSettings);

                    await httpContext.Response.WriteAsync(readingJson);
                };
            }
        }

        private void ListSensors(RouteContext context)
        {
            using (Log.OnEnterAndExit())
            {
                context.Handler = async httpContext =>
                {
                    var results = sensors.Select(s => s.Read());

                    await Task.WhenAll(results);

                    var readings = new Dictionary<string, object>();

                    // add a self link
                    var localPath = httpContext.Request.Path;
                    var links = new Dictionary<string, string>
                    {
                        { "self", localPath }
                    };

                    foreach (var sensorName in readings.Keys)
                    {
                        links.Add(
                            sensorName,
                            localPath.Add("/" + sensorName.ToLowerInvariant()).Value);
                    }

                    readings["_links"] = links;

                    var readingJson = JsonConvert.SerializeObject(readings, serializerSettings);

                    await httpContext.Response.WriteAsync(readingJson);
                };
            }
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }
    }
}
