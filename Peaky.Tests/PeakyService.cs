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
using Pocket;

namespace Peaky.Tests;

public class PeakyService : IDisposable
{
    private readonly TestServer testServer;
    private readonly CompositeDisposable disposables = new CompositeDisposable();

    public PeakyService(
        Action<TestTargetRegistry> configureTargets = null,
        Action<IServiceCollection> configureServices = null,
        Type[] testTypes = null)
    {
        testServer = Configure(
            configureTargets,
            configureServices,
            testTypes);

        disposables.Add(testServer);
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

        return new TestServer(webHostBuilder.UseStartup<TestApiStartup>());
    }

    public HttpClient CreateHttpClient()
    {
        var httpMessageHandler = testServer.CreateHandler();
        var clientHandler = new DelegatingHandlerWithCookies(httpMessageHandler);
        var httpClient = new HttpClient(clientHandler);
        return httpClient;
    }

    public void Dispose() => disposables.Dispose();

    internal class TestApiStartup
    {
        public TestApiStartup(IWebHostEnvironment environment)
        {
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => options.EnableEndpointRouting = false);

            return services.BuildServiceProvider();
        }

        public void Configure(
            IApplicationBuilder app,
            ILoggerFactory loggerFactory)
        {
            // FIX: (Configure)     loggerFactory.AddPocketLogger();

            app.UseMvc();

            app.UsePeaky();
        }
    }
}