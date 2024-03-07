// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Peaky;

public static class ApplicationBuilderExtensions
{
    private static readonly object lockObj = new object();

    public static IApplicationBuilder UsePeaky(this IApplicationBuilder app)
    {
        lock (lockObj)
        {            
            if (!Trace.Listeners.OfType<PeakyTraceListener>().Any())
            {
                Trace.Listeners.Add(new PeakyTraceListener());
            }
        }

        app.UseRouter(builder =>
        {
            var services = builder.ServiceProvider;

                
            var sensorRegistry = services.GetService<SensorRegistry>();

            if (sensorRegistry != null)
            {
                builder.Routes.Add(
                    new SensorRouter(
                            sensorRegistry,
                            services.GetRequiredService<AuthorizeSensors>())
                        .AllowVerbs("GET"));
            }

            var testTargets = services.GetService<TestTargetRegistry>();

            var testDefinitions = services.GetService<TestDefinitionRegistry>();

            if (testTargets != null &&
                testDefinitions != null)
            {
                var uiRouter = new TestPageRouter(services.GetRequiredService<ITestPageRenderer>())
                               .AllowVerbs("GET")
                               .Accept("text/html");
                builder.Routes.Add(uiRouter);

                var testRouter = new TestRouter(testTargets, testDefinitions)
                    .AllowVerbs("GET", "POST");
                builder.Routes.Add(testRouter);

                services.GetService<ILoggerFactory>()
                        ?.AddProvider(new PeakyLoggerProvider());
            }
        });

        return app;
    }
}