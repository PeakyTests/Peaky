// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Peaky.Tests
{
    public class SensorAuthorizationTests : IDisposable
    {
        private readonly HttpClient apiClient;
        private readonly PeakyService peakyService;
        private AuthorizeSensors authorize;

        public SensorAuthorizationTests()
        {
            peakyService = new PeakyService(
                configureServices: services => services
                    .AddPeakySensors(context =>
                    {
                        authorize?.Invoke(context);
                    }));

            apiClient = peakyService.CreateHttpClient();
        }

        public void Dispose() => peakyService.Dispose();

        [Fact]
        public async Task Authorization_can_be_denied_for_all_sensors()
        {
            authorize = context =>
            {
                context.Handler = async httpContext => httpContext.Response.StatusCode = 403;
            };

            var response = await apiClient.GetAsync("http://blammo.com/sensors/SensorMethod");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task POST_to_sensors_returns_405()
        {
            var response = await apiClient.PostAsync("http://blammo.com/sensors/", null);

            response.StatusCode
                    .Should()
                    .Be(HttpStatusCode.MethodNotAllowed);
        }
    }
}
