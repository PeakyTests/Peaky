using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Peaky
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UsePeaky(this IApplicationBuilder app)
        {
            if (!Trace.Listeners.OfType<PeakyTraceListener>().Any())
            {
                Trace.Listeners.Add(new PeakyTraceListener());
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
                    var uiRouter = new TestPageRouter(services.GetRequiredService<ITestPageFormatter>())
                        .AllowVerbs("GET")
                        .Accept("text/html");
                    builder.Routes.Add(uiRouter);

                    var testRouter = new TestRouter(testTargets, testDefinitions)
                        .AllowVerbs("GET", "POST");
                    builder.Routes.Add(testRouter);
                }
            });

            return app;
        }
    }
}
