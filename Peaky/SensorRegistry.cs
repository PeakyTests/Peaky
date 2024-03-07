// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Peaky;

public class SensorRegistry : Collection<DiagnosticSensor>
{
    public SensorRegistry()
    {
    }

    public SensorRegistry(IEnumerable<DiagnosticSensor> list) : base(list.ToList())
    {
    }

    /// <summary>
    ///   Registers the specified sensor.
    /// </summary>
    /// <param name="sensor"> A function that returns the sensor result. </param>
    /// <param name="name"> The name of the sensor. </param>
    public void Add<T>(Func<T> sensor, string name = null)
    {
        var anonymousMethodInfo = sensor.GetAnonymousMethodInfo();

        name = name ?? anonymousMethodInfo.MethodName;

        Add(new DiagnosticSensor(
                @delegate: sensor,
                returnType: typeof(T),
                name: name,
                declaringType: anonymousMethodInfo.EnclosingType));
    }

    public bool TryGet(string sensorName, out DiagnosticSensor sensor)
    {
        sensor = this.SingleOrDefault(s => s.Name.Equals(sensorName, StringComparison.OrdinalIgnoreCase));

        return sensor != null;
    }
}