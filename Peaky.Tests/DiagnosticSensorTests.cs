// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using FluentAssertions;
using System.Linq;
using System.Threading;
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
            var sensors = DiagnosticSensor.KnownSensors().Where(s => s.Name == "DictionarySensor");

            sensors.Should().HaveCount(1);
        }

        [Fact]
        public void Sensor_methods_can_be_internal()
        {
            var sensors = DiagnosticSensor.KnownSensors().Where(s => s.Name == "InternalSensor");

            sensors.Should().HaveCount(1);
        }

        [Fact]
        public void Sensor_methods_can_be_private()
        {
            var sensors = DiagnosticSensor.KnownSensors().Where(s => s.Name == "PrivateSensor");

            sensors.Should().HaveCount(1);
        }

        [Fact]
        public void Sensor_methods_can_be_static_methods()
        {
            var sensors = DiagnosticSensor.KnownSensors().Where(s => s.DeclaringType == typeof(StaticTestSensors));

            sensors.Count().Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        public void Sensor_methods_can_be_instance_methods()
        {
            var sensors = DiagnosticSensor.KnownSensors().Where(s => s.DeclaringType == typeof(TestSensors));

            sensors.Count().Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        public void Sensor_info_can_be_queried_based_on_sensor_declaring_type()
        {
            var sensors = DiagnosticSensor.KnownSensors().Where(s => s.DeclaringType == typeof(TestSensors));

            sensors.Count().Should().BeGreaterOrEqualTo(2);
        }

        [Fact]
        public void Sensors_can_be_queried_based_on_return_type()
        {
            var sensors = DiagnosticSensor.KnownSensors().Where(s => s.ReturnType == typeof(FileInfo));

            sensors.Should().HaveCount(1);
        }

        [Fact]
        public void Sensor_names_can_be_specified_using_DisplayNameAttribute()
        {
            DiagnosticSensor.KnownSensors()
                            .Count(s => s.Name == "custom-name")
                            .Should().Be(1);
        }

        [Fact]
        public void Sensor_methods_are_invoked_per_Read_call()
        {
            var first = DiagnosticSensor.KnownSensors().Single(s => s.Name == "CounterSensor").Read();
            var second = DiagnosticSensor.KnownSensors().Single(s => s.Name == "CounterSensor").Read();

            first.Should().NotBe(second);
        }

        [Fact]
        public void When_sensor_throws_an_exception_then_the_exception_is_returned()
        {
            var result = DiagnosticSensor.KnownSensors().Single(s => s.Name == "ExceptionSensor").Read();

            result.Should().BeOfType<Exception>();
        }

        [Fact]
        public void Sensors_can_be_registered_at_runtime()
        {
            var sensorName = nameof(Sensors_can_be_registered_at_runtime);
            DiagnosticSensor.Register(() => "hello", sensorName);

            var sensor = DiagnosticSensor.KnownSensors().Single(s => s.Name == sensorName);

            sensor.DeclaringType.Should().Be(GetType());
            sensor.ReturnType.Should().Be(typeof(string));
            sensor.Read().Should().Be("hello");
        }

        [Fact]
        public void When_registered_sensor_is_anonymous_then_default_name_is_derived_from_method_doing_registration()
        {
            var newGuid = Guid.NewGuid();
            DiagnosticSensor.Register(() => newGuid);

            var sensor = DiagnosticSensor.KnownSensors().Single(s => s.Read().Equals(newGuid));

            sensor.Name.Should().Contain(nameof(When_registered_sensor_is_anonymous_then_default_name_is_derived_from_method_doing_registration));
        }

        [Fact]
        public void When_registered_sensor_is_a_method_then_name_is_derived_from_its_name()
        {
            DiagnosticSensor.Register(SensorNameTester);

            DiagnosticSensor.KnownSensors().Count(s => s.Name == "SensorNameTester").Should().Be(1);
        }

        [Fact]
        public void When_registered_sensor_is_anonymous_then_DeclaringType_is_the_containing_type()
        {
            var newGuid = Guid.NewGuid();
            DiagnosticSensor.Register(() => newGuid);

            var sensor = DiagnosticSensor.KnownSensors().Single(s => s.Read().Equals(newGuid));

            sensor.DeclaringType.Should().Be(GetType());
        }

        [Fact]
        public void When_registered_sensor_is_a_method_then_DeclaringType_is_the_containing_type()
        {
            DiagnosticSensor.Register(StaticTestSensors.ExceptionSensor);

            var sensor = DiagnosticSensor.KnownSensors().Single(s => s.Name == "ExceptionSensor");

            sensor.DeclaringType.Should().Be(typeof(StaticTestSensors));
        }

        [Fact]
        public void Registered_sensors_can_be_removed_at_runtime()
        {
            DiagnosticSensor.Register(SensorNameTester);

            DiagnosticSensor.KnownSensors().Count(s => s.Name == "SensorNameTester").Should().Be(1);

            DiagnosticSensor.Remove("SensorNameTester");

            DiagnosticSensor.KnownSensors().Count(s => s.Name == "SensorNameTester").Should().Be(0);
        }

        [Fact]
        public void Discovered_sensors_can_be_removed_at_runtime()
        {
            DiagnosticSensor.KnownSensors().Count(s => s.Name == "Sensor_for_Discovered_sensors_can_be_removed_at_runtime").Should().Be(1);

            DiagnosticSensor.Remove("Sensor_for_Discovered_sensors_can_be_removed_at_runtime");

            DiagnosticSensor.KnownSensors().Count(s => s.Name == "Sensor_for_Discovered_sensors_can_be_removed_at_runtime").Should().Be(0);
        }

        [Fact]
        public void KnownSensors_is_safe_to_modify_while_being_enumerated()
        {
            StaticTestSensors.Barrier = new Barrier(2);
            DiagnosticSensor.Register(() => "hello", nameof(KnownSensors_is_safe_to_modify_while_being_enumerated));
            new Thread(() =>
            {
                DiagnosticSensor.KnownSensors().Select(s =>
                {
                    Console.WriteLine(s.Name);
                    return s.Read();
                }).ToArray();
            }).Start();

            StaticTestSensors.Barrier.SignalAndWait();

            DiagnosticSensor.Register(SensorNameTester);
            DiagnosticSensor.Remove(nameof(KnownSensors_is_safe_to_modify_while_being_enumerated));
        }

        [Fact]
        public void When_a_sensor_returns_a_Task_then_its_IsAsync_Property_returns_true()
        {
            var sensorName = Any.Paragraph(5);
            DiagnosticSensor.Register(AsyncTask, sensorName);

            var sensor = DiagnosticSensor.KnownSensors().Single(s => s.Name == sensorName);

            sensor.IsAsync.Should().BeTrue();
        }

        [Fact]
        public void When_a_sensor_does_not_return_a_Task_then_its_IsAsync_Property_returns_false()
        {
            var sensorName = Any.Paragraph(5);
            DiagnosticSensor.Register(() => "hi!", sensorName);

            var sensor = DiagnosticSensor.KnownSensors().Single(s => s.Name == sensorName);

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

    public class TestSensors
    {
        [DiagnosticSensor]
        public IDictionary<string, object> DictionarySensor()
        {
            return new Dictionary<string, object>
            {
                { "an int", 42 }
            };
        }

        [DiagnosticSensor]
        public FileInfo FileInfoSensor()
        {
            return new FileInfo(@"c:\somefile.txt");
        }
    }

    public static class StaticTestSensors
    {
        private static int callCount = 0;

        public static Barrier Barrier;

        [DiagnosticSensor]
        internal static object InternalSensor()
        {
            return new object();
        }

        [DiagnosticSensor]
        private static object PrivateSensor()
        {
            return new object();
        }

        [DiagnosticSensor]
        internal static object CounterSensor()
        {
            return callCount++;
        }

        [DiagnosticSensor]
        [DisplayName("custom-name")]
        public static object CustomNamedSensor()
        {
            return new object();
        }

        [DiagnosticSensor]
        public static object ExceptionSensor()
        {
            throw new InvalidOperationException();
        }

        [DiagnosticSensor]
        private static object Sensor_for_Discovered_sensors_can_be_removed_at_runtime()
        {
            return new object();
        }

        [DiagnosticSensor]
        private static object ConcurrencySensor()
        {
            Barrier.SignalAndWait();
            return new object();
        }
    }
}