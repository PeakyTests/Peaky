// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Threading;

namespace Peaky
{
    public class TraceBuffer
    {
        private const string TraceBufferKey = "Peaky.TraceBuffer";

        private readonly StringBuilder buffer = new StringBuilder();

        private static readonly AsyncLocal<TraceBuffer> current = new AsyncLocal<TraceBuffer>();

        public void Write(string message)
        {
            HasContent = true;
            buffer.AppendLine(message);
        }

        public bool HasContent { get; private set; }

        public override string ToString() => buffer.ToString();

        public static TraceBuffer Current
        {
            get
            {
                if (current.Value == null)
                {
                    current.Value = new TraceBuffer();
                }

                return current.Value;
            }
        }
    }
}