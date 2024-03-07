// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pocket;

namespace Peaky;

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

    private DiagnosticSensor(MethodInfo methodInfo) : this(
        methodInfo.ReturnType,
        methodInfo.Name,
        methodInfo.DeclaringType,
        methodInfo.CreateDelegate(typeof(Func<object>), null))
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
    public async Task<SensorResult> Read()
    {
        try
        {
            var value = @delegate.DynamicInvoke();

            if (value is Task valueTask)
            {
                await valueTask;

                if (valueTask.GetType().GetGenericArguments().First().IsVisible)
                {
                    value = ((dynamic) valueTask).Result;
                }
                else
                {
                    // this is required to work around the fact that internal types cause dynamic calls to Result to fail. JSON.NET however will happily serialize them, at which point we can retrieve the Result property.
                    var serialized = JsonConvert.SerializeObject(value, SerializerSettings);
                    value = JsonConvert.DeserializeObject<dynamic>(serialized).Result;
                }
            }

            return new SensorResult
            {
                SensorName = Name,
                Value = value
            };
        }
        catch (TargetInvocationException exception)
        {
            return new SensorResult
            {
                SensorName = Name,
                Exception = exception.InnerException
            };
        }
        catch (Exception exception)
        {
            return new SensorResult
            {
                SensorName = Name,
                Exception = exception
            };
        }
    }

    /// <summary>
    /// Discovers sensors found in all loaded assemblies.
    /// </summary>
    public static IReadOnlyCollection<DiagnosticSensor> DiscoverSensors()
    {
        return Discover
               .Types()
               .SelectMany(t =>
               {
                   return t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                       BindingFlags.Static)
                           .Where(m =>
                           {
                               IEnumerable<Attribute> customAttributes;

                               try
                               {
                                   customAttributes = m.GetCustomAttributes();
                               }
                               catch
                               {
                                   return false;
                               }

                               return customAttributes
                                   .Any(a =>
                                   {
                                       return a.GetType().Name.Equals("DiagnosticSensor") ||
                                              a.GetType().Name.Equals("DiagnosticSensorAttribute");
                                   });
                           });
               })
               .Select(m => new DiagnosticSensor(m))
               .ToArray();
    }

    /// <summary>
    /// Gets the name of a sensor.
    /// </summary>
    /// <param name="sensorMethod">The sensor method.</param>
    /// <returns>The sensor's name</returns>
    /// <remarks>The sensor name can be set by decorating the sensor method with <see cref="DisplayNameAttribute" />. Otherwise, the name of the method is used.</remarks>
    internal static string GetName(MethodInfo sensorMethod)
    {
        var displayName = sensorMethod
                          .GetCustomAttributes(typeof(DisplayNameAttribute), false)
                          .OfType<DisplayNameAttribute>()
                          .FirstOrDefault();

        return displayName != null
                   ? displayName.DisplayName
                   : sensorMethod.Name;
    }

    internal static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Error = (sender, args) =>
        {
            args.ErrorContext.Handled = true;
        }
    };
}