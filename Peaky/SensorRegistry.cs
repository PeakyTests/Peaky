using System;
using System.Collections.ObjectModel;

namespace Peaky
{
    public class SensorRegistry : Collection<DiagnosticSensor>
    {
        /// <summary>
        ///   Registers the specified sensor.
        /// </summary>
        /// <param name="sensor"> A function that returns the sensor result. </param>
        /// <param name="name"> The name of the sensor. </param>
        public  void Register<T>(Func<T> sensor, string name = null)
        {
            var anonymousMethodInfo = sensor.GetAnonymousMethodInfo();

            name = name ?? anonymousMethodInfo.MethodName;

            Add(new DiagnosticSensor(
                    @delegate: sensor,
                    returnType: typeof(T),
                    name: name,
                    declaringType: anonymousMethodInfo.EnclosingType));
        }
    }
}