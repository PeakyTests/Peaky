// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Its.Recipes;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Peaky.Tests
{
    public class SensorErrorTests : IDisposable
    {
        private static HttpClient apiClient;
        private readonly string sensorName;
        private readonly PeakyService peakyService;
        private readonly SensorRegistry registry;

        public SensorErrorTests()
        {
            registry = new SensorRegistry();
            peakyService = new PeakyService(
                configureServices: s => s.AddSingleton(registry));
            apiClient = peakyService.CreateHttpClient();
            sensorName = Any.AlphanumericString(10, 20);
            apiClient = peakyService.CreateHttpClient();
        }

        public void Dispose()
        {
            peakyService.Dispose();
            TestSensor.GetSensorValue = null;
        }

        [Fact]
        public void when_a_specific_sensor_is_requested_and_it_throws_then_it_returns_500_and_the_exception_text_is_in_the_response_body()
        {
            registry.Add<string>(() => throw new Exception("oops!"), sensorName);

            var result = apiClient.GetAsync("http://blammo.com/sensors/" + sensorName).Result;

            var body = result.Content.ReadAsStringAsync().Result;

            result.StatusCode
                  .Should()
                  .Be(HttpStatusCode.InternalServerError);

            body.Should().Contain("oops!");
        }

        [Fact]
        public void when_all_sensors_are_requested_and_one_throws_then_it_returns_200_and_the_exception_text_is_in_the_response_body()
        {
            registry.Add<string>(() => throw new Exception("oops!"), sensorName);

            var result = apiClient.GetAsync("http://blammo.com/sensors/").Result;

            var body = result.Content.ReadAsStringAsync().Result;

            result.StatusCode
                  .Should().Be(HttpStatusCode.OK);
            body.Should().Contain("oops!");
        }

        [Fact]
        public void Cyclical_references_in_object_graphs_do_not_cause_serialization_errors()
        {
            var registry = new SensorRegistry();
            registry.Add(() =>
            {
                var parent = new Node();
                parent.ChildNodes.Add(new Node
                {
                    ChildNodes = new List<Node> { parent }
                });
                return parent;
            }, sensorName);

            apiClient.GetAsync("http://blammo.com/sensors/").Result
                     .StatusCode
                     .Should()
                     .Be(HttpStatusCode.OK);
            apiClient.GetAsync("http://blammo.com/sensors/" + sensorName).Result
                     .StatusCode
                     .Should()
                     .Be(HttpStatusCode.OK);
        }

        public class Node
        {
            public List<Node> ChildNodes = new List<Node>();
        }
    }
}
