// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Peaky;

public static class AssertionExtensions
{
    public static HttpResponseMessage ShouldSucceed(
        this HttpResponseMessage response,
        HttpStatusCode? expected = null)
    {
        try
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Response does not indicate success: {response.StatusCode}: {response.ReasonPhrase}");
            }

            var actualStatusCode = response.StatusCode;
            if (expected is not null && actualStatusCode != expected.Value)
            {
                throw new TestFailedException(
                    $"Status code was successful but not of the expected type: {expected} was expected but {actualStatusCode} was returned.");
            }
        }
        catch
        {
            ThrowVerboseAssertion(response);
        }

        return response;
    }

    public static async Task<HttpResponseMessage> ShouldSucceedAsync(
        this Task<HttpResponseMessage> response,
        HttpStatusCode? expected = null) =>
        (await response).ShouldSucceed(expected);

    public static HttpResponseMessage ShouldFailWith(
        this HttpResponseMessage response,
        HttpStatusCode code)
    {
        if (response.StatusCode != code)
        {
            ThrowVerboseAssertion(response);
        }

        return response;
    }

    public static async Task<HttpResponseMessage> ShouldFailWithAsync(
        this Task<HttpResponseMessage> response,
        HttpStatusCode code) =>
        (await response).ShouldFailWith(code);

    public static Aggregation<IEnumerable<T>, long> CountOf<T>(this IEnumerable<T> sequence, Func<T, bool> selector)
    {
        var filteredResults = sequence.Where(selector).ToArray();

        return new Aggregation<IEnumerable<T>, long>(filteredResults, filteredResults.Length);
    }

    public static Aggregation<IEnumerable<T>, Percentage> PercentageOf<T>(this IEnumerable<T> sequence, Func<T, bool> selector)
    {
        var count = 0;

        var filteredResults = sequence.Where(_ =>
        {
            count++;
            return selector(_);
        }).ToArray();

        var percentage = count == 0 ? 0 : (double)filteredResults.Length / count;

        return new Aggregation<IEnumerable<T>, Percentage>(filteredResults, new Percentage((int)(percentage * 100)));
    }

    public static AggregationAssertions<TState, TResult> Should<TState, TResult>(this Aggregation<TState, TResult> aggregation) where TResult : IComparable<TResult>
    {
        return new AggregationAssertions<TState, TResult>(aggregation);
    }

    public static Percentage Percent(this int i)
    {
        return new Percentage(i);
    }

    private static void ThrowVerboseAssertion(HttpResponseMessage response)
    {
        throw new TestFailedException(response.ToString());
    }
}