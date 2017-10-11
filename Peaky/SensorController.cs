// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Threading.Tasks;
using Its.Recipes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Peaky
{
    /// <summary>
    /// Exposes sensors discovered in all loaded assemblies via HTTP endpoints.
    /// </summary>
    [AuthorizeSensors]
    public class SensorController : Controller
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        /// <summary>
        /// Reads all sensors.
        /// </summary>
        /// <returns>The values returned by all sensors.</returns>
        public async Task<IActionResult> Get()
        {
            var readings = DiagnosticSensor.KnownSensors().ToDictionary(s => s.Name, s => s.Read());

            var asyncSensors = readings.Where(pair => pair.Value is Task).ToArray();

            if (asyncSensors.Any())
            {
                await Task.WhenAll(asyncSensors.Select(pair => pair.Value).OfType<Task>());
            }

            foreach (var pair in asyncSensors)
            {
                readings[pair.Key] = ((dynamic) pair.Value).Result;
            }

            // add a self link
            var localPath = Request.Path;
            var links = new Dictionary<string, string>
            {
                { "self", localPath }
            };

            foreach (var sensorName in readings.Keys)
            {
                links.Add(sensorName, localPath.Add(sensorName.ToLowerInvariant()));
            }

            readings["_links"] = links;

            return Ok(readings);
        }

        /// <summary>
        /// Reads the specified sensor.
        /// </summary>
        /// <param name="name">The name of the sensor to read.</param>
        /// <returns>The value returned by the sensor.</returns>
        public async Task<IActionResult> Get(string name)
        {
            var sensor = DiagnosticSensor
                .KnownSensors()
                .SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

            if (sensor == null)
            {
                return NotFound();
            }

            var reading = sensor.Read();

            var readingTask = reading as Task;
            if (readingTask != null)
            {
                await readingTask;
                if (readingTask.GetType().GetTypeInfo().GetGenericArguments().First().GetTypeInfo().IsVisible)
                {
                    reading = ((dynamic) readingTask).Result;
                }
                else
                {
                    // this is required to work around the fact that internal types cause dynamic calls to Result to fail. JSON.NET however will happily serialize them, at which point we can retrieve the Result property.
                    var serialized = JsonConvert.SerializeObject(reading, jsonSerializerSettings);
                    reading = JsonConvert.DeserializeObject<dynamic>(serialized).Result;
                }
            }

            // add a self link
            var localPath = Request.Path;
            reading
                .IfTypeIs<IDictionary<string, object>>()
                .ThenDo(d => d["_links"] = new { self = localPath })
                .ElseDo(() =>
                {
                    var json = JsonConvert.SerializeObject(reading, jsonSerializerSettings);

                    if (!json.Contains("{"))
                    {
                        json = @"{""value"":" + json + @"}";
                    }

                    var jtoken = JsonConvert.DeserializeObject<JToken>(json);

                    jtoken.IfTypeIs<JObject>()
                          .ThenDo(o => o.Add("_links", new JObject(new JProperty("self", localPath))));

                    reading = jtoken;
                });

            if (reading is Exception)
            {
                throw new Exception();
            }

            return Ok(reading);
        }
    }
}