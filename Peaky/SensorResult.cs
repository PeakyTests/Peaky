// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Peaky;

public class SensorResult
{
    internal SensorResult()
    {
    }

    public object Value { get; internal set; }

    public Exception Exception { get; internal set; }

    public string SensorName { get; internal set; }
}