// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Peaky.Tests
{
    public class AssertionExtensionsTests
    {
        [Fact]
        public void When_ShouldSucceed_is_passed_a_failed_response_it_throws()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

            Action assert = () => response.ShouldSucceed();

            assert.Should().Throw<TestFailedException>();
        }

        [Fact]
        public void When_ShouldFailWith_is_passed_a_successful_response_it_throws()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            Action assert = () => response.ShouldFailWith(HttpStatusCode.Forbidden);

            assert.Should().Throw<TestFailedException>();
        }

        [Fact]
        public void When_ShouldSucceed_is_passed_a_successful_response_it_doesnt_throw()
        {
            var response = new HttpResponseMessage(HttpStatusCode.Accepted);

            Action assert = () => response.ShouldSucceed();

            assert.Should().NotThrow();
        }

        [Fact]
        public void When_ShouldFailWith_is_passed_a_failed_response_it_doesnt_throw()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

            Action assert = () => response.ShouldFailWith(HttpStatusCode.BadRequest);

            assert.Should().NotThrow();
        }

        [Fact]
        public async Task When_ShouldSucceedAsync_is_passed_a_failed_response_it_throws()
        {
            var response = Task.Run(() => new HttpResponseMessage(HttpStatusCode.BadRequest));

            Func<Task> assert = () => response.ShouldSucceedAsync();

            await assert.Should().ThrowAsync<TestFailedException>();
        }

        [Fact]
        public async Task When_ShouldFailWithAsync_is_passed_a_successful_response_it_throws()
        {
            var response = Task.Run(() => new HttpResponseMessage(HttpStatusCode.OK));

            Func<Task> assert = () => response.ShouldFailWithAsync(HttpStatusCode.Forbidden);

            await assert.Should().ThrowAsync<TestFailedException>();
        }

        [Fact]
        public async Task When_ShouldSucceedAsync_is_passed_a_successful_response_it_doesnt_throw()
        {
            var response = Task.Run(() => new HttpResponseMessage(HttpStatusCode.Accepted));

            Func<Task> assert = async () =>
            {
                await response.ShouldSucceedAsync();
            };

            await assert.Should().NotThrowAsync();
        }

        [Fact]
        public async Task When_ShouldFailWithAsync_is_passed_a_failed_response_it_doesnt_throw()
        {
            var response = Task.Run(() => new HttpResponseMessage(HttpStatusCode.BadRequest));

            Func<Task> assert = async () =>
            {
                var x = await response.ShouldFailWithAsync(HttpStatusCode.BadRequest);
            };

            await assert.Should().NotThrowAsync();
        }
    }
}