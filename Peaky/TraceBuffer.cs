// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Threading;

namespace Peaky
{
    internal class TraceBuffer
    {
        private readonly StringBuilder buffer = new StringBuilder();

        private static readonly AsyncLocal<TraceBuffer> current = new AsyncLocal<TraceBuffer>();

        public void Write(string message) => buffer.Append(message);

        public override string ToString() => buffer.ToString();

        public static void Initialize() => current.Value = new TraceBuffer();

        public static TraceBuffer Current => current.Value;
    }
}