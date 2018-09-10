// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Peaky
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPeakyTests(
            this IServiceCollection builder,
            Action<TestTargetRegistry> configure = null,
            IEnumerable<Type> testTypes = null)
        {
            builder.TryAddSingleton(c =>
            {
                var registry = new TestTargetRegistry(c);

                configure?.Invoke(registry);

                return registry;
            });

            builder.TryAddSingleton(c => new TestDefinitionRegistry(testTypes));

            builder.TryAddTransient<ITestPageRenderer>(c => new TestPageRenderer());
            
            builder.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return builder;
        }

        public static IServiceCollection AddPeakySensors(
            this IServiceCollection builder,
            AuthorizeSensors authorize = null,
            string baseUri = "sensors")
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            authorize = authorize ?? (_ =>
            {
            });

            builder.TryAddSingleton(authorize);

            builder.TryAddSingleton(c => new SensorRegistry(DiagnosticSensor.DiscoverSensors()));

            return builder;
        }
    }
}
