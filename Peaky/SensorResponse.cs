// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;

namespace Peaky;

internal class SensorResponse : SensorResult
{
    private readonly string localPath;

    public SensorResponse(SensorResult reading, string localPath)
    {
        this.localPath = localPath;
        Value = reading.Value;
        Exception = reading.Exception;
        SensorName = reading.SensorName;
    }

    public object _links => new { self = localPath };

    public int StatusCode =>
        Exception == null
            ? (int) HttpStatusCode.OK
            : (int) HttpStatusCode.InternalServerError;
}