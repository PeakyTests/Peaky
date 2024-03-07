// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Peaky;

public class Telemetry
{
    public int ElapsedMilliseconds { get; set; }
    public string OperationName { get; set; }
    public bool Succeeded { get; set; }
    public string UserIdentifier { get; set; }
    public Dictionary<string, object> Properties { get; set; }
}