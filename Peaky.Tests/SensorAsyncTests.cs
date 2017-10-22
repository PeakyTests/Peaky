// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Its.Recipes;
using Microsoft.Extensions.DependencyInjection;
using Pocket;
using Xunit;

namespace Peaky.Tests
{
    public class SensorAsyncTests : IDisposable
    {
        private static HttpClient apiClient;
        private readonly string sensorName;
        private readonly SensorRegistry registry;
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public SensorAsyncTests()
        {
            registry = new SensorRegistry(DiagnosticSensor.DiscoverSensors());

            var peaky = new PeakyService(configureServices: s => s.AddSingleton(registry));

            apiClient = peaky.CreateHttpClient();

            sensorName = Any.AlphanumericString(10, 20);
        }

        public void Dispose()
        {
            disposables.Dispose();
            TestSensor.GetSensorValue = null;
        }

        [Fact]
        public async Task When_a_sensor_returning_a_Task_of_anonymous_type_is_requested_specifically_then_the_Result_is_returned()
        {
            var words = Any.Paragraph(5);
            var sensorResult = new
            {
                words
            };
            registry.Add(() => Task.Run(() => sensorResult), sensorName);

            var result = await apiClient.GetStringAsync("http://blammo.com/sensors/" + sensorName);

            result.Should().Contain("words");
            result.Should().Contain(words);
        }

        [Fact]
        public async Task When_a_sensor_returning_a_Task_is_requested_specifically_then_the_Result_is_returned()
        {
            var words = Any.Paragraph(5);
            registry.Add(() => Task.Run(() => words), sensorName);

            var result = await apiClient.GetStringAsync("http://blammo.com/sensors/" + sensorName);

            result.Should()
                  .Contain(words);
        }

        [Fact]
        public async Task When_all_sensors_are_requested_then_the_Result_values_are_returned_for_those_that_return_Tasks()
        {
            registry.Add(() => Task.Run(() => "task result"), sensorName);

            var result = await apiClient.GetStringAsync("http://blammo.com/sensors/");

            result.Should()
                  .Contain(string.Format("\"{0}\":{{\"Value\":\"{1}\"", sensorName, "task result"));
        }

        [Fact]
        public void When_all_sensors_are_requested_and_some_are_slow_async_they_are_combined_with_synchronous_sensors()
        {
            registry.Add(() => "fast sensor result", "fast sensor");

            registry.Add(() => Task.Run(async () =>
            {
                await Task.Delay(1000);
                return "slow sensor result";
            }), "slow sensor");

            var result = apiClient.GetStringAsync("http://blammo.com/sensors/").Result;

            result.Should()
                  .Contain("\"slow sensor\":{\"Value\":\"slow sensor result\"")
                  .And
                  .Contain("\"fast sensor\":{\"Value\":\"fast sensor result\"");
        }
    }
}
