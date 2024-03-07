// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Peaky;

public static class TestDependencyRegistryExtensions
{
    public static TestDependencyRegistry UseServiceWarmup<T>(this TestDependencyRegistry registry)
        where T : IPerformServiceWarmup
    {
        registry.Container.RegisterSingle(c => new ServiceWarmupTracker(c.Resolve<T>()));

        return registry;
    }
}