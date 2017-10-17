using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

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
                var sensorRegistry = builder.ServiceProvider.GetService<SensorRegistry>();

                if (sensorRegistry != null)
                {
                    builder.Routes.Add(
                        new SensorRouter(
                            sensorRegistry,
                            builder.ServiceProvider.GetRequiredService<AuthorizeSensors>()
                        ).AllowVerbs("GET"));
                }

                var testTargets = builder.ServiceProvider.GetService<TestTargetRegistry>();

                var testDefinitions = builder.ServiceProvider.GetService<TestDefinitionRegistry>();

                if (testTargets != null &&
                    testDefinitions != null)
                {
                    var uiRouter = new TestUIRouter()
                        .AllowVerbs("GET")
                        .Accept(new MediaTypeHeaderValue("text/html"));
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
