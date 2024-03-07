// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pocket;
using Xunit;
using Xunit.Abstractions;

namespace Peaky.Tests
{
    public class TestDependencyTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public TestDependencyTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task Target_environment_is_available_by_declaring_a_dependency_on_Target_when_no_resolver_is_specified()
        {
            var api = new PeakyService(targets => targets.Add("staging", "widgetapi", new Uri("http://localhost:81")));

            var response = await api.CreateHttpClient().GetAsync("http://blammo.com/tests/staging/widgetapi/get_target");

            response.ShouldSucceed(HttpStatusCode.OK);

            var result = await response.Content.ReadAsStringAsync();

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
            var api = new PeakyService(targets => targets.Add("production", "widgetapi", new Uri("http://localhost:81")));

            var response = await api.CreateHttpClient().GetAsync("http://blammo.com/tests/production/widgetapi/unsatisfiable_dependencies_test");

            var message = await response.Content.ReadAsStringAsync();

            response.ShouldFailWith(HttpStatusCode.InternalServerError);

            message.Should()
                   .Contain(
                       "\"ClassName\":\"System.InvalidOperationException\",\"Message\":\"TestTarget does not contain registration for 'System.Collections.Generic.List`1[System.Collections.Generic.KeyValuePair`2[System.Nullable`1[System.DateTimeOffset],System.Collections.Generic.HashSet`1[System.Guid]]]'.\"");
        }

        [Fact]
        public void Test_targets_require_absolute_URIs()
        {
            var configure = () =>
            {
                new PeakyService(targets =>
                                     targets.Add("this", "that", new Uri("/relative/uri", UriKind.Relative)));
            };

            configure.Should()
                     .Throw<ArgumentException>()
                     .Which
                     .Message
                     .Should()
                     .Contain("Base address must be an absolute URI");
        }

        [Fact]
        public async Task HttpClient_is_configured_by_default_using_TestTarget_BaseAddress()
        {
            var api = new PeakyService(targets => targets.Add("production", "widgetapi", new Uri("http://localhost:42")));

            var response = await api.CreateHttpClient().GetAsync("http://blammo.com/tests/production/widgetapi/HttpClient_BaseAddress");

            var message = await response.Content.ReadAsStringAsync();

            response.ShouldSucceed();

            message.Should().Contain("BaseAddress = http://localhost:42");
        }

        [Fact]
        public async Task When_HttpClient_BaseAddress_is_set_in_dependency_registration_then_it_is_not_overridden()
        {
            var api = new PeakyService(targets =>
                                           targets
                                               .Add("production", "widgetapi", new Uri("http://google.com"),
                                                    dependencies => dependencies.Register(() => new HttpClient
                                                    {
                                                        BaseAddress = new Uri("http://bing.com")
                                                    })));

            var response = await api.CreateHttpClient().GetAsync("http://blammo.com/tests/production/widgetapi/HttpClient_BaseAddress");

            var message = await response.Content.ReadAsStringAsync();

            Console.WriteLine(message);

            response.ShouldSucceed();

            message.Should().Contain("BaseAddress = http://bing.com");
        }

        [Fact]
        public async Task When_HttpClient_BaseAddress_is_not_set_in_dependency_registration_then_it_is_set_to_the_test_target_configured_value()
        {
            var api = new PeakyService(targets =>
                                           targets
                                               .Add("production", "widgetapi", new Uri("http://bing.com"),
                                                    dependencies => dependencies.Register(() => new HttpClient())));

            var response = await api.CreateHttpClient().GetAsync("http://blammo.com/tests/production/widgetapi/HttpClient_BaseAddress");

            var message = await response.Content.ReadAsStringAsync();

            response.ShouldSucceed();

            message.Should().Contain("BaseAddress = http://bing.com");
        }

        [Fact]
        public async Task Dependencies_added_to_ServiceProvider_are_resolvable_by_Peaky()
        {
            var peaky = new PeakyService(
                configureTargets: targets => targets.Add("production", "widgetapi", new Uri("http://blammo.com")),
                configureServices: services =>
                    services.AddTransient<IList<string>>(c =>
                                                             new List<string>
                                                             {
                                                                 "one",
                                                                 "two",
                                                                 "three"
                                                             }),
                testTypes: new[] { typeof(TestWithDependencyOn<IList<string>>) });

            var response = await peaky.CreateHttpClient().GetAsync("http://blammo.com/tests/production/widgetapi/dependency_test");

            var testResult = await response.AsTestResult();

            JsonConvert.DeserializeObject<string[]>(testResult.ReturnValue.ToString()).Should()
                  .BeEquivalentTo("one",
                                  "two",
                                  "three");
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

    public class TestWithDependencyOn<T> : IPeakyTest
    {
        private readonly T dependency;

        public TestWithDependencyOn(T dependency)
        {
            this.dependency = dependency;
        }

        public async Task<T> dependency_test()
        {
            await Task.Yield();
            return dependency;
        }
    }

    public class TestsWithDependencyOnTarget : IPeakyTest
    {
        private readonly TestTarget testTarget;

        public TestsWithDependencyOnTarget(TestTarget testTarget)
        {
            this.testTarget = testTarget ??
                              throw new ArgumentNullException(nameof(testTarget));
        }

        public Task<dynamic> get_target() => Task.FromResult<dynamic>(testTarget);
    }

    public class TestWithUnsatisfiableDependencies : IPeakyTest
    {
        public TestWithUnsatisfiableDependencies(List<KeyValuePair<DateTimeOffset?, HashSet<Guid>>> probablyNotRegistered)
        {
        }

        public void unsatisfiable_dependencies_test()
        {
        }
    }
}
