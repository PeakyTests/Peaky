// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Peaky
{
    internal class PeakyLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) =>
            new PeakyLogger(categoryName);

        public void Dispose()
        {
        }
    }
}
