// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Its.Recipes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Peaky.Tests
{
    public class PeakyTestExecutionForAttributedRoutedTests
    {
        private static HttpClient apiClient;

        public PeakyTestExecutionForAttributedRoutedTests()
        {
//            configuration = new HttpConfiguration();
//            var constraintResolver = new DefaultInlineConstraintResolver();
//            constraintResolver.ConstraintMap.Add("among", typeof (AmongConstraint));
//            configuration.MapHttpAttributeRoutes(constraintResolver);
//            configuration.MapTestRoutes();
//            configuration.EnsureInitialized();
//
//            var server = new HttpServer(configuration);


            var server = new PeakyServer();
            apiClient = server.CreateTestServer().CreateClient();
        }

        [Fact]
        public void When_a_test_passes_then_a_200_is_returned()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/pass").Result;

            response.ShouldSucceed(HttpStatusCode.OK);
        }

        [Fact]
        public void When_a_test_passes_then_the_response_contains_the_test_output()
        {
            var value = Any.CamelCaseName();

            var response = apiClient.GetAsync("http://blammo.com/tests/pass/" + value).Result;

            var result = response.Content.ReadAsStringAsync().Result;

            result.Should().Contain(value);
        }

        [Fact]
        public void When_a_test_throws_then_a_500_Test_Failed_is_returned()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/fail").Result;

            response.ShouldFailWith(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void When_a_test_fails_then_the_response_contains_the_test_output_and_exception_details()
        {
            var response = apiClient.GetAsync("http://blammo.com/tests/fail").Result;

            var result = response.Content.ReadAsStringAsync().Result;

            result.Should().Contain("oops!");
        }
    }

    public class ContainsTestsController : Controller
    {
        [HttpGet]
        [Route("tests/{environment:regex(^(done)|(working)$)}/Healthy", Name = "healthy")]
        public dynamic Healthy(string environment)
        {
            return environment + " is healthy!";
        }

        [HttpGet]
        [Route("tests/Fail", Name = "fail")]
        public dynamic Fail()
        {
            throw new Exception("oops!");
        }

        [HttpGet]
        [Route("tests/Pass/{value:maxlength(1000)?}", Name = "pass")]
        public dynamic Pass(string value = null)
        {
            return value;
        }

        [HttpGet]
        [Route("tests/sideeffectingtest/{environment:among(done,working)}", Name = "sideeffectingtest")]
        public dynamic SideEffectingTest(string environment)
        {
            return environment;
        }
    }

    public class DoesntContainTestsController : Controller
    {
        [HttpGet]
        [Route("api/dostuff")]
        public void DoStuff()
        {
        }
    }
}