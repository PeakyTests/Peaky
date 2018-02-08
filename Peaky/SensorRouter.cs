// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Pocket;
using static Peaky.DiagnosticSensor;
using static Pocket.Logger<Peaky.SensorRouter>;

namespace Peaky
{
    internal class SensorRouter : PeakyRouter
    {
        private readonly AuthorizeSensors authorizeSensors;

        private readonly SensorRegistry sensors;

        public SensorRouter(SensorRegistry sensors, AuthorizeSensors authorizeSensors, string pathBase = "/sensors") : base(pathBase)
        {
            this.authorizeSensors = authorizeSensors ??
                                    throw new ArgumentNullException(nameof(authorizeSensors));
            this.sensors = sensors ??
                           throw new ArgumentNullException(nameof(sensors));
        }

        public override async Task RouteAsync(RouteContext context)
        {
            authorizeSensors(context);
            if (context.Handler != null)
            {
                return;
            }

            var segments = context.HttpContext
                                  .Request
                                  .Path
                                  .Value
                                  .Substring(PathBase.Value.Length)
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

                    var sensorResponse = new SensorResponse(reading, httpContext.Request.Path.Value);

                    httpContext.Response.StatusCode = sensorResponse.StatusCode;

                    var readingJson = JsonConvert.SerializeObject(sensorResponse, SerializerSettings);

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
                    var results = await Task.WhenAll(sensors.Select(s => s.Read()));

                    var readings = results.ToDictionary(s => s.SensorName, s => (object) s);

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

                    var readingJson = JsonConvert.SerializeObject(readings, SerializerSettings);

                    await httpContext.Response.WriteAsync(readingJson);
                };
            }
        }
    }
}
