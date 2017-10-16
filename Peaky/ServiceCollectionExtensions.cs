using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Peaky
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UsePeaky(this IApplicationBuilder app)
        {
            app.UseRouter(builder =>
            {
                var sensorRegistry = builder.ServiceProvider.GetService<SensorRegistry>();

                if (sensorRegistry != null)
                {
                    builder.Routes.Add(new SensorRouter(sensorRegistry));
                }

                var testTargets = builder.ServiceProvider.GetService<TestTargetRegistry>();
                var testDefinitions = builder.ServiceProvider.GetService<TestDefinitionRegistry>();

                if (testTargets != null)
                {
                    builder.Routes.Add(
                        new TestRouter(
                        testTargets, 
                        testDefinitions));
                }
            });

            return app;
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPeakyTests(
            this IServiceCollection builder,
            Action<TestTargetRegistry> configure = null,
            IEnumerable<Type> testTypes = null)
        {
            builder.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap.Add("among", typeof(AmongConstraint));
            });

            builder.AddSingleton(c =>
            {
                var registry = new TestTargetRegistry();

                configure?.Invoke(registry);

                return registry;
            });

            builder.AddSingleton(c =>
            {
                return new TestDefinitionRegistry(testTypes);
            });

            return builder;
        }

        public static IServiceCollection AddPeakySensors(
            this IServiceCollection builder,
            Func<AuthorizationFilterContext, bool> authorizeRequest = null,
            string baseUri = "sensors")
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            authorizeRequest = authorizeRequest ?? (_ => true);

            builder.AddSingleton(c =>
            {
                return DiagnosticSensor.DiscoverSensors();
            });

            return builder;
        }
    }
}
