using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace Peaky
{
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
            return new FileInfo("somefile.txt");
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
            throw new DataMisalignedException();
        }

        [DiagnosticSensor]
        private static object Sensor_for_Discovered_sensors_can_be_removed_at_runtime()
        {
            return new object();
        }

        [DiagnosticSensor]
        private static object ConcurrencySensor()
        {
            Barrier?.SignalAndWait();
            return new object();
        }
    }
}