// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace Peaky
{
    internal class PeakyTraceListener : TraceListener
    {
        public override void Write(string message) =>
            TraceBuffer.Current?.Write(message);

        public override void WriteLine(string message) =>
            TraceBuffer.Current?.Write(message + Environment.NewLine);
    }
}
