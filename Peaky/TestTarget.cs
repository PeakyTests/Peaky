// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Peaky
{
    public class TestTarget
    {
        public TestTarget(Func<Type, object> resolveDependency)
        {
            ResolveDependency = resolveDependency ?? throw new ArgumentNullException(nameof(resolveDependency));
        }

        public string Application { get; internal set; }

        public string Environment { get; internal set; }

        public Uri BaseAddress { get; internal set; }

        internal Func<Type, object> ResolveDependency { get; }
    }
}