// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Its.Recipes;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Peaky.Tests
{
    public class SensorAsyncTests : IDisposable
    {
        private static HttpClient peakyService;
        private readonly string sensorName;
        private readonly SensorRegistry registry;

        public SensorAsyncTests()
        {
            registry = new SensorRegistry(DiagnosticSensor.DiscoverSensors());

            peakyService = new PeakyService(
                configureServices: s => s.AddSingleton(registry)).CreateHttpClient();

            sensorName = Any.AlphanumericString(10, 20);
        }

        public void Dispose()
        {
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

            var result = await peakyService.GetStringAsync("http://blammo.com/sensors/" + sensorName);

            result.Should().Contain("words");
            result.Should().Contain(words);
        }

        [Fact]
        public async Task When_a_sensor_returning_a_Task_is_requested_specifically_then_the_Result_is_returned()
        {
            var words = Any.Paragraph(5);
            registry.Add(() => Task.Run(() => words), sensorName);

            var result = await peakyService.GetStringAsync("http://blammo.com/sensors/" + sensorName);

            result.Should()
                  .Contain(words);
        }

        [Fact]
        public async Task When_all_sensors_are_requested_then_the_Result_values_are_returned_for_those_that_return_Tasks()
        {
            var words = Any.Paragraph(5);
            TestSensor.GetSensorValue = () =>
            {
                return Task.Run(() => words);
            };

            var result = await peakyService.GetStringAsync("http://blammo.com/sensors/");

            result.Should()
                  .Contain(string.Format("\"SensorMethod\":\"{0}\"", words));
        }

        [Fact]
        public void When_all_sensors_are_requested_and_some_are_slow_async_they_are_combined_with_synchronous_sensors()
        {
            var testSensor = Any.Paragraph(5);
            var dynamicSensor = Any.Paragraph(5);

            TestSensor.GetSensorValue = () => Task.Run(() =>
            {
                Thread.Sleep(Any.Int(1000, 2000));
                return testSensor;
            });

            registry.Add(() => Task.Run(() =>
            {
                Thread.Sleep(Any.Int(3000, 5000));
                return dynamicSensor;
            }), sensorName);

            var result = peakyService.GetStringAsync("http://blammo.com/sensors/").Result;

            result.Should()
                  .Contain(string.Format("\"SensorMethod\":\"{0}\"", testSensor))
                  .And
                  .Contain(string.Format("\"{0}\":\"{1}\"", sensorName, dynamicSensor));
        }
    }
}
