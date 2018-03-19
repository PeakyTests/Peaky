// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pocket.For.MicrosoftExtensionsLogging;

namespace Peaky.Tests
{
    public class PeakyService : IDisposable
    {
        private readonly TestServer testServer;
        private HttpClient httpClient;

        public PeakyService(
            Action<TestTargetRegistry> configureTargets = null,
            Action<IServiceCollection> configureServices = null,
            Type[] testTypes = null)
        {
            testServer = Configure(
                configureTargets,
                configureServices,
                testTypes);
        }

        private static TestServer Configure(
            Action<TestTargetRegistry> configureTargets = null,
            Action<IServiceCollection> configureServices = null,
            IReadOnlyCollection<Type> testTypes = null)
        {
            var webHostBuilder = new WebHostBuilder();

            if (configureServices != null)
            {
                webHostBuilder.ConfigureServices(configureServices);
            }

            webHostBuilder.ConfigureServices(services =>
            {
                services.AddPeakySensors();
                services.AddPeakyTests(configureTargets, testTypes);
            });

            return new TestServer(
                webHostBuilder
                    .UseStartup<TestApiStartup>());
        }

        public HttpClient CreateHttpClient() => httpClient ?? (httpClient = testServer.CreateClient());

        public void Dispose() => httpClient.Dispose();

        internal class TestApiStartup
        {
            public TestApiStartup(IHostingEnvironment environment)
            {
            }

            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                services.AddMvc();

                return services.BuildServiceProvider();
            }

            public void Configure(
                IApplicationBuilder app,
                ILoggerFactory loggerFactory)
            {
                loggerFactory.AddPocketLogger();

                app.UseMvc();

                app.UsePeaky();
            }
        }
    }
}
