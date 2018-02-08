// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Logging;
using Pocket;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Peaky
{
    internal class PeakyLogger : ILogger
    {
        private readonly string categoryName;

        public PeakyLogger(string categoryName)
        {
            this.categoryName = categoryName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            TraceBuffer.Current?.Write(formatter(state, exception));

        public bool IsEnabled(LogLevel logLevel) =>
            TraceBuffer.Current != null;

        public IDisposable BeginScope<TState>(TState state) =>
            Disposable.Empty;
    }
}
