// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Its.Log.Instrumentation;

namespace Peaky
{
    public class TestTarget
    {
        static TestTarget()
        {
            Formatter<TestTarget>.RegisterForAllMembers();  
        }

        public TestTarget(Func<Type, object> resolveDependency)
        {
            if (resolveDependency == null)
            {
                throw new ArgumentNullException(nameof(resolveDependency));
            }
            ResolveDependency = resolveDependency;
        }

        public string Application { get; internal set; }

        public string Environment { get; internal set; }

        public Uri BaseAddress { get; internal set; }

        internal Func<Type, object> ResolveDependency { get; private set; }

        public override string ToString() => this.ToLogString();
    }
}