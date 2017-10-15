// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pocket;
using Xunit;
using Xunit.Abstractions;

namespace Peaky.Tests
{
    public class PeakyTestDependencyTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public PeakyTestDependencyTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public void Target_environment_is_available_by_declaring_a_dependency_on_Target_when_no_resolver_is_specified()
        {
            var api = new PeakyService(targets => targets.Add("staging", "widgetapi", new Uri("http://localhost:81")));

            var response = api.CreateHttpClient().GetAsync("http://blammo.com/tests/staging/widgetapi/get_target").Result;

            response.ShouldSucceed(HttpStatusCode.OK);

            var result = response.Content.ReadAsStringAsync().Result;

            string environment = JsonConvert.DeserializeObject<dynamic>(result)
                                            .ReturnValue
                                            .Environment;

            environment.Should().Be("staging");
        }

        [Fact]
        public async Task Dependencies_can_be_declared_that_are_specific_to_environment_and_application()
        {
            var api = new PeakyService(
                    targets => targets
                        .Add("production",
                             "widgets",
                             new Uri("http://widgets.com"),
                             t => t.Register<HttpClient>(() => new FakeHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK))
                             {
                                 BaseAddress = new Uri("http://widgets.com")
                             }))
                        .Add("staging",
                             "widgets",
                             new Uri("http://staging.widgets.com"),
                             t => t.Register<HttpClient>(() => new FakeHttpClient(_ => new HttpResponseMessage(HttpStatusCode.GatewayTimeout))
                             {
                                 BaseAddress = new Uri("http://staging.widgets.com")
                             })))
                .CreateHttpClient();

            // try production, which should be reachable
            var response = api.GetAsync("http://blammo.com/tests/production/widgets/is_reachable");
            await response.ShouldSucceedAsync();

            // then staging, which should not be reachable
            response = api.GetAsync("http://blammo.com/tests/staging/widgets/is_reachable");
            await response.ShouldFailWithAsync(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task When_a_test_cannot_be_instantiated_due_to_missing_dependencies_then_the_URL_is_still_displayed()
        {
            var api = new PeakyService(targets => targets.Add("production", "widgetapi", new Uri("http://localhost:81")));

            var response = await api.CreateHttpClient().GetAsync("http://blammo.com/tests/production/widgetapi");

            response.ShouldSucceed();

            var testList = await response.AsTestList();

            testList.Tests
                    .Should()
                    .Contain(o =>
                                 o.Url == "http://blammo.com/tests/production/widgetapi/unsatisfiable_dependencies_test");
        }

        [Fact]
        public async Task When_a_test_cannot_be_instantiated_due_to_missing_dependencies_then_calling_the_error_test_returns_500_with_details()
        {
            var api = new PeakyService(configureTargets: targets => targets.Add("production", "widgetapi", new Uri("http://localhost:81")));

            var response = await api.CreateHttpClient().GetAsync("http://blammo.com/tests/production/widgetapi/unsatisfiable_dependencies_test");

            var message = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(message);

            response.ShouldFailWith(HttpStatusCode.InternalServerError);

            message.Should()
                   .Contain(
                       "{\"ClassName\":\"System.ArgumentException\",\"Message\":\"PocketContainer can\'t construct a System.Collections.Generic.IEnumerable`1[System.Collections.Generic.KeyValuePair`2[System.Nullable`1[System.DateTimeOffset],System.Collections.Generic.HashSet`1[System.Guid]]] unless you register it first. ☹\"");
        }

        [Fact]
        public void Test_targets_require_absolute_URIs()
        {
            Action configure = () =>
            {
                new PeakyService(targets =>
                                     targets.Add("this", "that", new Uri("/relative/uri", UriKind.Relative)));
            };

            configure.ShouldThrow<ArgumentException>()
               .Which
               .Message
               .Should()
               .Contain("Base address must be an absolute URI");
        }

        [Fact]
        public void HttpClient_is_configured_by_default_using_TestTarget_BaseAddress()
        {
            var api = new PeakyService(targets => targets.Add("production", "widgetapi", new Uri("http://localhost:42")));

            var response = api.CreateHttpClient().GetAsync("http://blammo.com/tests/production/widgetapi/HttpClient_BaseAddress").Result;

            var message = response.Content.ReadAsStringAsync().Result;

            response.ShouldSucceed();

            message.Should().Contain("BaseAddress = http://localhost:42");
        }

        [Fact]
        public void When_HttpClient_BaseAddress_is_set_in_dependency_registration_then_it_is_not_overridden()
        {
            var api = new PeakyService(configureTargets: targets =>
                                           targets
                                               .Add("production", "widgetapi", new Uri("http://google.com"),
                                                    dependencies => dependencies.Register(() => new HttpClient
                                                    {
                                                        BaseAddress = new Uri("http://bing.com")
                                                    })));

            var response = api.CreateHttpClient().GetAsync("http://blammo.com/tests/production/widgetapi/HttpClient_BaseAddress").Result;

            var message = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine(message);

            response.ShouldSucceed();

            message.Should().Contain("BaseAddress = http://bing.com");
        }

        [Fact]
        public void When_HttpClient_BaseAddress_is_not_set_in_dependency_registration_then_it_is_set_to_the_test_target_configured_value()
        {
            var api = new PeakyService(configureTargets: targets =>
                                           targets
                                               .Add("production", "widgetapi", new Uri("http://bing.com"),
                                                    dependencies => dependencies.Register(() => new HttpClient())));

            var response = api.CreateHttpClient().GetAsync("http://blammo.com/tests/production/widgetapi/HttpClient_BaseAddress").Result;

            var message = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine(message);

            response.ShouldSucceed();

            message.Should().Contain("BaseAddress = http://bing.com");
        }
    }

    public class TestsWithDependencies : IPeakyTest
    {
        private readonly HttpClient httpClient;
        private readonly TestTarget testTarget;

        public TestsWithDependencies(HttpClient httpClient, TestTarget testTarget)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.testTarget = testTarget ?? throw new ArgumentNullException(nameof(testTarget));
        }

        public async Task<dynamic> is_reachable() => 
            await httpClient.GetAsync("/sensors").ShouldSucceedAsync();

        public string HttpClient_BaseAddress() => 
            "BaseAddress = " + httpClient.BaseAddress;
    }

    public class TestsWithDependencyOnTarget : IPeakyTest
    {
        private readonly TestTarget testTarget;

        public TestsWithDependencyOnTarget(TestTarget testTarget)
        {
            this.testTarget = testTarget ??
                throw new ArgumentNullException(nameof(testTarget));
        }

        public async Task<dynamic> get_target() => testTarget;
    }

    public class TestWithUnsatisfiableDependencies : IPeakyTest
    {
        public TestWithUnsatisfiableDependencies(IEnumerable<KeyValuePair<DateTimeOffset?, HashSet<Guid>>> probablyNotRegistered)
        {
        }

        public void unsatisfiable_dependencies_test()
        {
        }
    }
}
