// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Pocket;
using Xunit;
using Xunit.Abstractions;

namespace Peaky.Tests
{
    public class ServiceWarmupTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();
        private readonly PeakyService peakyService;

        public ServiceWarmupTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));

            peakyService = new PeakyService(
                targets => targets
                    .Add("production",
                         "widgetapi",
                         new Uri("http://widgets.com"),
                        dependencies => dependencies.UseServiceWarmup<TestWarmup>() )
                );

            disposables.Add(peakyService);

            disposables.Add(Disposable.Create(TestWarmup.ResetCount));
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task When_a_service_is_slow_to_warm_up_then_IPerformServiceWarmup_allows_tests_to_wait_until_warmup_has_happened()
        {
            var response = await peakyService.CreateHttpClient()
                                             .GetAsync("http://blammo.com/tests/production/widgetapi/Slow_test");

            response.ShouldSucceed(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Warm_up_status_is_cached()
        {
            await peakyService.CreateHttpClient()
                              .GetAsync("http://blammo.com/tests/production/widgetapi/Fast_test");
            await peakyService.CreateHttpClient()
                              .GetAsync("http://blammo.com/tests/production/widgetapi/Slow_test");

           TestWarmup.WarmupCount.Should().Be(1);
        }
    }

    public class TestsThatNeedServiceWarmup : IPeakyTest
    {
        public async Task<string> Slow_test()
        {
            await Task.Yield();

            if (TestWarmup.WarmupCount == 0)
            {
                throw new TimeoutException("oops");
            }

            return "done!";
        }

        public async Task<string> Fast_test()
        {
            await Task.Yield();

            return "done!";
        }
    }

    public class TestWarmup : IPerformServiceWarmup
    {
        public static int WarmupCount { get; private set; }

        public static void ResetCount() => WarmupCount = 0;

        public Task WarmUp()
        {
            WarmupCount++;
            return Task.CompletedTask;
        }
    }
}
