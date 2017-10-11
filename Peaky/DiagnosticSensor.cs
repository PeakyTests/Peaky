// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;

namespace Peaky
{
    /// <summary>
    ///   Provides diagnostic information on demand.
    /// </summary>
    public class DiagnosticSensor
    {
        private readonly Delegate @delegate;
        private readonly string name;
        private readonly Type declaringType;
        private readonly Type returnType;
        private bool isAsync;

        internal DiagnosticSensor(Type returnType, string name, Type declaringType, Delegate @delegate)
        {
            this.returnType = returnType;
            this.name = name;
            this.declaringType = declaringType;
            this.@delegate = @delegate;

            ValidateAndInitialize();
        }

        private DiagnosticSensor(MethodInfo methodInfo) : this(methodInfo.ReturnType, methodInfo.Name, methodInfo.DeclaringType, methodInfo.CreateDelegate(typeof(Func<object>)))
        {
        }

        private void ValidateAndInitialize()
        {
            if (returnType == null)
            {
                throw new ArgumentNullException(nameof(returnType));
            }
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (declaringType == null)
            {
                throw new ArgumentNullException(nameof(declaringType));
            }
            if (@delegate == null)
            {
                throw new ArgumentNullException(nameof(@delegate));
            }

            isAsync = typeof(Task).IsAssignableFrom(returnType);
        }

        /// <summary>
        ///   Gets the type on which the sensor method is declared.
        /// </summary>
        /// <value> The type of the declaring. </value>
        public Type DeclaringType => declaringType;

        /// <summary>
        ///   Gets the name of the sensor method.
        /// </summary>
        public string Name => name;

        /// <summary>
        ///   Gets the return type of the sensor method.
        /// </summary>
        /// <value> The type of the return. </value>
        public Type ReturnType => returnType;

        /// <summary>
        /// Gets a value indicating whether the sensor's return value is async.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the sensor is async; otherwise, <c>false</c>.
        /// </value>
        public bool IsAsync => isAsync;

        /// <summary>
        ///   Reads the sensor and returns its value.
        /// </summary>
        /// <remarks>
        ///   If the sensor throws an exception, the exception is returned.
        /// </remarks>
        public object Read()
        {
            try
            {
                return @delegate.DynamicInvoke();
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private static readonly Lazy<ConcurrentDictionary<string, DiagnosticSensor>> allSensors =
            new Lazy<ConcurrentDictionary<string, DiagnosticSensor>>(Discover);

        /// <summary>
        /// Discovers sensors found in all loaded assemblies.
        /// </summary>
        public static ConcurrentDictionary<string, DiagnosticSensor> Discover()
        {
            var enumerable = Pocket.Discover
                                   .ConcreteTypes()
                                   .SelectMany(t => t.GetTypeInfo()
                                                     .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                                                 BindingFlags.Static)
                                                     .Where(m => m.CustomAttributes
                                                                  .Any(a => a.GetType().Name.Equals("DiagnosticSensor") || a.GetType().Name.Equals("DiagnosticSensorAttribute"))))
                                   .Select(m => new DiagnosticSensor(m));
            return enumerable
                .OrderBy(sensor => sensor.Name)
                .ThenBy(sensor => sensor.DeclaringType.GetTypeInfo().Assembly.FullName)
                .Aggregate(new ConcurrentDictionary<string, DiagnosticSensor>(), (sensors, sensor) =>
                {
                    // FIX: (Discover) we should be graceful about collisions or deterministic about ordering
                    sensors[sensor.Name] = sensor;
                    return sensors;
                });
        }

        /// <summary>
        /// Gets the name of a sensor.
        /// </summary>
        /// <param name="sensorMethod">The sensor method.</param>
        /// <returns>The sensor's name</returns>
        /// <remarks>The sensor name can be set by decorating the sensor method with <see cref="DisplayNameAttribute" />. Otherwise, the name of the method is used.</remarks>
        public static string GetName(MethodInfo sensorMethod)
        {
            var displayName = sensorMethod
                .GetCustomAttributes(typeof(DisplayNameAttribute), false)
                .OfType<DisplayNameAttribute>()
                .FirstOrDefault();

            return displayName != null
                       ? displayName.DisplayName
                       : sensorMethod.Name;
        }

        /// <summary>
        ///   Returns all of the diagnostic sensors found in the application.
        /// </summary>
        public static IEnumerable<DiagnosticSensor> KnownSensors() => allSensors.Value.Values.ToArray();

        /// <summary>
        ///   Registers the specified sensor.
        /// </summary>
        /// <param name="sensor"> A function that returns the sensor result. </param>
        /// <param name="name"> The name of the sensor. </param>
        public static void Register<T>(Func<T> sensor, string name = null)
        {
            name = name ?? sensor.GetMethodInfo().Name;
            allSensors.Value[name] = new DiagnosticSensor(
                @delegate: sensor,
                returnType: typeof(T),
                name: name,
                declaringType: sensor.GetMethodInfo().DeclaringType);
        }

        /// <summary>
        ///   Removes any sensor having the specified name.
        /// </summary>
        /// <param name="name"> The sensor name. </param>
        public static void Remove(string name) => allSensors.Value.TryRemove(name, out DiagnosticSensor _);
    }
}