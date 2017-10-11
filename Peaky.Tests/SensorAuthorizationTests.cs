// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Its.Recipes;
using Xunit;

namespace Peaky.Tests
{
    public class SensorAuthorizationTests : IDisposable
    {
        private string sensorName;

        public SensorAuthorizationTests()
        {
            sensorName = Any.AlphanumericString(10, 20);
        }

        public void Dispose()
        {
            DiagnosticSensor.Remove(sensorName);
            TestSensor.GetSensorValue = null;
        }

        [Fact]
        public void Authorization_can_be_denied_for_all_sensors()
        {
//            var configuration = new HttpConfiguration().MapSensorRoutes(ctx => false);
//            var apiClient = new HttpClient(new HttpServer(configuration));
//            DiagnosticSensor.Register(() => "hello", sensorName);
//
//            apiClient.GetAsync("http://blammo.com/sensors/" + sensorName).Result.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            // FIX: (Authorization_can_be_denied_for_all_sensors) 
            throw new NotImplementedException();
        }

        [Fact]
        public void Authorization_can_be_denied_for_specific_sensors()
        {
//            var configuration = new HttpConfiguration
//            {
//                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always
//            }.MapSensorRoutes(ctx =>
//            {
//                var name = (string) ctx.ControllerContext.RouteData.Values["name"];
//                return !string.Equals(name, sensorName, StringComparison.OrdinalIgnoreCase);
//            });
//            var apiClient = new HttpClient(new HttpServer(configuration));
//            DiagnosticSensor.Register(() => "hello", sensorName);
//
//            apiClient.GetAsync("http://blammo.com/sensors/" + sensorName)
//                     .Result
//                     .ShouldFailWith(HttpStatusCode.Forbidden);
//            apiClient.GetAsync("http://blammo.com/sensors/application")
//                     .Result
//                     .ShouldSucceed();

            // FIX: (Authorization_can_be_denied_for_specific_sensors) 
            throw new NotImplementedException();
        }

        [Fact]
        public void POST_to_sensors_returns_405()
        {
//            var configuration = new HttpConfiguration().MapSensorRoutes(ctx => true);
//            var apiClient = new HttpClient(new HttpServer(configuration));
//
//            apiClient.PostAsync("http://blammo.com/sensors/", null).Result.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);

            // FIX: (POST_to_sensors_returns_405) 
            throw new NotImplementedException();
        }

        [Fact]
        public void A_message_handler_can_be_specified_to_perform_authentication_prior_to_the_sensor_authorization_check()
        {
//            var authenticator = new Authenticator
//            {
//                Send = request =>
//                {
//                    var response = request.CreateResponse(HttpStatusCode.OK);
//                    response.Headers.Add("authenticated", "true");
//                    return response;
//                }
//            };
//            var httpConfig = new HttpConfiguration();
//
//            HttpMessageHandler handler = HttpClientFactory.CreatePipeline(
//                new HttpControllerDispatcher(httpConfig),
//                new[] { authenticator });
//
//            httpConfig.MapSensorRoutes(
//                ctx => ctx.Response.Headers.Contains("authenticated") &&
//                       ctx.Response.Headers.GetValues("authenticated").Single() == "true",
//                handler: handler);
//
//            var apiClient = new HttpClient(new HttpServer(httpConfig));
//
//            apiClient.GetAsync("http://blammo.biz/sensors").Result.StatusCode.Should().Be(HttpStatusCode.OK);

            // FIX: (A_message_handler_can_be_specified_to_perform_authentication_prior_to_the_sensor_authorization_check) 
            throw new NotImplementedException();
        }

    }
}