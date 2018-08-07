// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Its.Recipes;
using Xunit;

namespace Peaky
{
    public class DiagnosticSensorTests : IDisposable
    {
        public void Dispose()
        {
            if (StaticTestSensors.Barrier != null)
            {
                StaticTestSensors.Barrier.RemoveParticipants(StaticTestSensors.Barrier.ParticipantCount);
                StaticTestSensors.Barrier = null;
            }
        }

        [Fact]
        public void Sensors_can_be_queried_based_on_sensor_name()
        {
            var sensors = DiagnosticSensor.DiscoverSensors().Where(s => s.Name == "DictionarySensor");

            sensors.Should().HaveCount(1);
        }

        [Fact]
        public void Sensor_methods_can_be_internal()
        {
            var sensors = DiagnosticSensor.DiscoverSensors().Where(s => s.Name == "InternalSensor");

            sensors.Should().HaveCount(1);
        }

        [Fact]
        public void Sensor_methods_can_be_private()
        {
            var sensors = DiagnosticSensor.DiscoverSensors().Where(s => s.Name == "PrivateSensor");

            sensors.Should().HaveCount(1);
        }

        [Fact]
        public void Sensor_methods_can_be_static_methods()
        {
            var sensors = DiagnosticSensor.DiscoverSensors().Where(s => s.DeclaringType == typeof(StaticTestSensors));

            sensors.Count().Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        public void Sensor_methods_can_be_instance_methods()
        {
            var sensors = DiagnosticSensor.DiscoverSensors().Where(s => s.DeclaringType == typeof(TestSensors));

            sensors.Count().Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        public void Sensor_info_can_be_queried_based_on_sensor_declaring_type()
        {
            var sensors = DiagnosticSensor.DiscoverSensors().Where(s => s.DeclaringType == typeof(TestSensors));

            sensors.Count().Should().BeGreaterOrEqualTo(2);
        }

        [Fact]
        public void Sensors_can_be_queried_based_on_return_type()
        {
            var sensors = DiagnosticSensor.DiscoverSensors().Where(s => s.ReturnType == typeof(FileInfo));

            sensors.Should().HaveCount(1);
        }

        [Fact]
        public void Sensor_methods_are_invoked_per_Read_call()
        {
            var first = DiagnosticSensor.DiscoverSensors().Single(s => s.Name == "CounterSensor").Read().Result;
            var second = DiagnosticSensor.DiscoverSensors().Single(s => s.Name == "CounterSensor").Read().Result;

            first.Should().NotBe(second);
        }

        [Fact]
        public void When_sensor_throws_an_exception_then_the_exception_is_returned()
        {
            var result = DiagnosticSensor.DiscoverSensors()
                                         .Single(s => s.Name == "ExceptionSensor")
                                         .Read()
                                         .Result
                                         .Exception;

            result.Should().BeOfType<DataMisalignedException>();
        }

        [Fact]
        public async Task Sensors_can_be_registered_at_runtime()
        {
            var sensorName = nameof(Sensors_can_be_registered_at_runtime);
            var registry = new SensorRegistry {{() => "hello", sensorName}};

            var sensor = registry.Single(s => s.Name == sensorName);

            sensor.DeclaringType.Should().Be(GetType());
            sensor.ReturnType.Should().Be(typeof(string));
            var reading = await sensor.Read();
            reading.Value.Should().Be("hello");
        }

        [Fact]
        public void When_registered_sensor_is_anonymous_then_default_name_is_derived_from_method_doing_registration()
        {
            var newGuid = Guid.NewGuid();

            var registry = new SensorRegistry {() => newGuid};

            var sensor = registry.Single(s => s.Read().Result.Value.Equals(newGuid));

            sensor.Name.Should().Contain(nameof(When_registered_sensor_is_anonymous_then_default_name_is_derived_from_method_doing_registration));
        }

        [Fact]
        public void When_registered_sensor_is_a_method_then_name_is_derived_from_its_name()
        {
            var registry = new SensorRegistry {SensorNameTester};

            registry.Count(s => s.Name == "SensorNameTester").Should().Be(1);}

        [Fact]
        public void When_registered_sensor_is_anonymous_then_DeclaringType_is_the_containing_type()
        {
            var newGuid = Guid.NewGuid();

            var registry = new SensorRegistry {() => newGuid};

            var sensor = registry.Single(s => s.Read().Result.Value.Equals(newGuid));

            sensor.DeclaringType.Should().Be(GetType());
        }

        [Fact]
        public void When_registered_sensor_is_a_method_then_DeclaringType_is_the_containing_type()
        {
            var registry = new SensorRegistry {StaticTestSensors.ExceptionSensor};

            var sensor = registry.Single(s => s.Name == "ExceptionSensor");

            sensor.DeclaringType.Should().Be(typeof(StaticTestSensors));
        }

        [Fact]
        public void When_a_sensor_returns_a_Task_then_its_IsAsync_Property_returns_true()
        {
            var sensorName = Any.Paragraph(5);
            var registry = new SensorRegistry {{AsyncTask, sensorName}};

            var sensor = registry.Single(s => s.Name == sensorName);

            sensor.IsAsync.Should().BeTrue();
        }

        [Fact]
        public void When_a_sensor_does_not_return_a_Task_then_its_IsAsync_Property_returns_false()
        {
            var sensorName = Any.Paragraph(5);
            var registry = new SensorRegistry {{() => "hi!", sensorName}};

            var sensor = registry.Single(s => s.Name == sensorName);

            sensor.IsAsync.Should().BeFalse();
        }

        public static async Task<dynamic> AsyncTask()
        {
            var One = Task.Run(() => "one");

            var Two = Task.Run(() => 2);

            return new
            {
                One = await One,
                Two = await Two
            };
        }

        private object SensorNameTester()
        {
            return new object();
        }
    }
}
