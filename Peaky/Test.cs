// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Peaky;

public class Test
{
    internal Test(string name, string environment, string application, string url)
    {
        Name = name;
        Environment = environment;
        Application = application;
        Url = url;
    }

    public string Application { get; }

    public string Environment { get; }

    public string Name { get; }

    public string Url { get; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string[] Tags { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TestParameter[] Parameters { get; init; }

    internal static IEnumerable<Test> CreateTests(TestTarget testTarget, TestDefinition definition, HttpRequest request)
    {
        var testCases = testTarget
                        .DependencyRegistry
                        .GetParameterSetsFor(definition.TestMethod);

        if (testCases.Any())
        {
            foreach (var testCase in testCases)
            {
                yield return new Test(
                    definition.TestName,
                    testTarget.Environment,
                    testTarget.Application,
                    request.GetLinkWithQuery(testTarget, definition, testCase))
                {
                    Tags = definition.Tags,
                    Parameters = testCase.ToArray()
                };
            }
        }
        else
        {
            yield return new Test(
                definition.TestName,
                testTarget.Environment,
                testTarget.Application,
                definition.Parameters.Any()
                    ? request.GetLinkWithQuery(testTarget, definition, definition.Parameters)
                    : request.GetLink(testTarget, definition))
            {
                Tags = definition.Tags,
                Parameters = definition.Parameters.Any()
                                 ? definition.Parameters.ToArray()
                                 : Array.Empty<TestParameter>()
            };
        }
    }
}