// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Its.Recipes;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Pocket;
using Xunit;

namespace Peaky.Tests
{
    public class SensorRoutingTests : IDisposable
    {
        private static HttpClient apiClient;
        private readonly string sensorName;
        private readonly SensorRegistry registry;
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public SensorRoutingTests()
        {
            registry = new SensorRegistry(DiagnosticSensor.DiscoverSensors());

            var peaky = new PeakyService(configureServices: s => s.AddSingleton(registry));

            apiClient = peaky.CreateHttpClient();

            sensorName = Any.AlphanumericString(10, 20);

            disposables.Add(peaky);
        }

        public void Dispose()
        {
            disposables.Dispose();
            TestSensor.GetSensorValue = null;
        }

        [Fact]
        public async Task Sensors_are_available_by_name()
        {
            var words = Any.Paragraph(5);

            registry.Add(() => new { words }, sensorName);

            var response = await apiClient.GetStringAsync("http://blammo.com/sensors/" + sensorName);

            response.Should().Contain(words);
        }

        [Fact]
        public async Task All_sensors_can_be_queried_at_once()
        {
            var value = Any.AlphanumericString(20, 100);
            TestSensor.GetSensorValue = () => value;

            var response = await apiClient.GetStringAsync("http://blammo.com/sensors/");

            Console.WriteLine(response);

            response.Should().Contain(value);
        }

        [Fact]
        public async Task Sensor_root_content_includes_link_to_sensors_root()
        {
            var response = await apiClient.GetStringAsync("http://blammo.com/sensors/");

            dynamic sensorValue = JObject.Parse(response);

            string selfLink = sensorValue._links.self;

            selfLink.Should().Be("/sensors/");
        }

        [Fact]
        public async Task Sensor_root_content_contains_links_to_all_sensors()
        {
            dynamic result = JObject.Parse(await apiClient.GetStringAsync("http://blammo.com/sensors/"));

            ((IEnumerable<dynamic>) result._links)
                .Select(l => l.Name)
                .Should()
                .BeEquivalentTo(registry.Select(s => s.Name).Concat(new[] { "self" }));
        }

        [Fact]
        public async Task Specific_sensor_content_includes_link_to_sensor_when_sensor_is_dictionary()
        {
            registry.Add(() => new Dictionary<string,object>
            {
                ["key"] = "value"
            }, sensorName);

            var response = await apiClient.GetStringAsync("http://blammo.com/sensors/" + sensorName);

            dynamic sensorValue = JObject.Parse(response);
            Console.WriteLine(response);
            string selfLink = sensorValue._links.self;

            selfLink.Should().Be("/sensors/" + sensorName);
        }

        [Fact]
        public async Task Specific_sensor_content_includes_link_to_sensor_when_sensor_is_object()
        {
            registry.Add(() => new FileInfo("c:\\temp\\foo.txt"), sensorName);
            var response = await apiClient.GetStringAsync("http://blammo.com/sensors/" + sensorName);

            dynamic sensorValue = JObject.Parse(response);

            string selfLink = sensorValue._links.self;

            selfLink.Should().Be("/sensors/" + sensorName);
        }

        [Fact]
        public async Task When_a_sensor_name_is_unknown_then_the_api_returns_404()
        {
            var sensorName = Guid.NewGuid();

            var response = await apiClient.GetAsync("http://blammo.com/sensors/" + sensorName);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Sensor_routes_are_case_insensitive()
        {
            registry.Add(() => "hi", "APPLICATION");

            (await apiClient.GetAsync("http://blammo.com/sensors/application"))
                      .StatusCode
                      .Should()
                      .Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Sensors_that_return_arrays_are_correctly_serialized()
        {
            var value = new
            {
                Name = Any.FullName(),
                Email = Any.Email()
            };
            TestSensor.GetSensorValue = () => new object[] { value };

            var response = await apiClient.GetAsync("http://blammo.com/sensors/sensormethod");

            response.ShouldSucceed();

            Console.WriteLine(await response.Content.ReadAsStringAsync());

            var values = await response.JsonContent();
            string name = values.Value[0].Name;
            string email = values.Value[0].Email;

            name.Should().Be(value.Name);
            email.Should().Be(value.Email);
        }

        [Fact]
        public async Task When_one_sensor_throws_then_other_sensors_are_not_affected()
        {
            TestSensor.GetSensorValue = () => throw new Exception("oops!");

            var response = await apiClient.GetAsync("http://blammo.com/sensors/sensormethod");

            response.ShouldFailWith(HttpStatusCode.InternalServerError);

            var body = await response.Content.ReadAsStringAsync();

            body.Should().Contain("oops!");
            body.Should().Contain(@"""_links"":");
        }
    }
}
