// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Peaky
{
    public class TestTarget
    {
        public TestTarget(TestDependencyRegistry testDependencyRegistry)
        {
            DependencyRegistry = testDependencyRegistry ?? throw new ArgumentNullException(nameof(testDependencyRegistry));
        }

        public string Application { get; internal set; }

        public string Environment { get; internal set; }

        public Uri BaseAddress { get; internal set; }

        public TestDependencyRegistry DependencyRegistry { get; }
    }
}