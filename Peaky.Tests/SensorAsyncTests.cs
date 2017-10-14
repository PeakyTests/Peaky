// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Its.Recipes;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Peaky.Tests
{
    public class SensorAsyncTests : IDisposable
    {
        private static HttpClient apiClient;
        private readonly string sensorName;
        private SensorRegistry registry;

        public SensorAsyncTests()
        {
            apiClient = new PeakyService().CreateHttpClient();
            sensorName = Any.AlphanumericString(10, 20);
            registry = new SensorRegistry();
        }

        public void Dispose()
        {
            TestSensor.GetSensorValue = null;
        }

        [Fact]
        public void When_a_sensor_returning_a_Task_of_anonymous_type_is_requested_specifically_then_the_Result_is_returned()
        {
            var words = Any.Paragraph(5);
            var sensorResult = new
            {
                words
            };
            registry.Register(() => Task.Run(() => sensorResult), sensorName);

            dynamic result = JObject.Parse(apiClient.GetStringAsync("http://blammo.com/sensors/" + sensorName).Result);

            ((string) result.words)
                .Should()
                .Be(words);
        }

        [Fact]
        public void When_a_sensor_returning_a_Task_is_requested_specifically_then_the_Result_is_returned()
        {
            var words = Any.Paragraph(5);
            registry.Register(() => Task.Run(() => words), sensorName);

            dynamic result = JObject.Parse(apiClient.GetStringAsync("http://blammo.com/sensors/" + sensorName).Result);

            ((string) result.value)
                .Should()
                .Be(words);
        }

        [Fact]
        public void When_all_sensors_are_requested_then_the_Result_values_are_returned_for_those_that_return_Tasks()
        {
            var words = Any.Paragraph(5);
            TestSensor.GetSensorValue = () => Task.Run(() => words);

            var result = apiClient.GetStringAsync("http://blammo.com/sensors/").Result;

            Console.WriteLine(result);

            result.Should()
                  .Contain(string.Format("\"SensorMethod\":\"{0}\"", words));
        }

        [Fact]
        public void When_all_sensors_are_requested_and_some_are_slow_async_they_are_combined_with_synchronous_sensors()
        {
            var testSensor = Any.Paragraph(5);
            var dynamicSensor = Any.Paragraph(7);

            TestSensor.GetSensorValue = () => Task.Run(() =>
            {
                Thread.Sleep(Any.Int(3000, 5000));
                return testSensor;
            });

            var registry = new SensorRegistry();

            registry.Register(() => Task.Run(() =>
            {
                Thread.Sleep(Any.Int(3000, 5000));
                return dynamicSensor;
            }), sensorName);

            var result = apiClient.GetStringAsync("http://blammo.com/sensors/").Result;

            Console.WriteLine(result);

            result.Should()
                  .Contain(string.Format("\"SensorMethod\":\"{0}\"", testSensor))
                  .And
                  .Contain(string.Format("\"{0}\":\"{1}\"", sensorName, dynamicSensor));
        }
    }
}