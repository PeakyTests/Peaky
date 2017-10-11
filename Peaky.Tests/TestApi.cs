// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace Peaky.Tests
{
    public class TestApi : IDisposable
    {
        private readonly TestServer testServer;
        private HttpClient httpClient;

        public TestApi(
            Action<TestTargetRegistry> configureTargets = null,
            params Type[] testTypes)
        {
            testServer = Configure(configureTargets, testTypes);
        }

        private static TestServer Configure(
            Action<TestTargetRegistry> configureTargets = null,
            params Type[] testTypes)
        {
            return new TestServer(new WebHostBuilder().UseStartup<TestApiStartup>());
        }

        public HttpClient CreateHttpClient()
        {
            return httpClient ?? (httpClient = testServer.CreateClient());
        }

        public void Dispose() => httpClient.Dispose();
    }

    internal class TestApiStartup
    {




    }
}