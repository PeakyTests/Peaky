// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Pocket;

namespace Peaky
{
    public class TestDependencyRegistry
    {
        internal TestDependencyRegistry(PocketContainer container)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
        }

        internal PocketContainer Container { get; }

        public TestDependencyRegistry Register<T>(Func<T> getDependency)
        {
            Container.Register(typeof(T), c => getDependency());

            return this;
        }
    }
}
