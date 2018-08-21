// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Peaky
{
    internal class Test
    {
        public string Application { get; set; }

        public string Environment { get; set; }

        public string Url { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] Tags { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Parameter[] Parameters { get; set; }

        public static IEnumerable<Test> CreateTests(TestTarget testTarget, TestDefinition definition, HttpRequest request)
        {
            var testCases = testTarget.DependencyRegistry.GetParameterSetsFor(definition.TestMethod)?.ToList();

            if (testCases != null && testCases.Count > 0)
            {
                foreach (var testCase in testCases)
                {
                    yield return new Test
                    {
                        Environment = testTarget.Environment,
                        Application = testTarget.Application,
                        Url = request.GetLinkWithQuery(testTarget, definition,testCase),
                        Tags = definition.Tags,
                        Parameters = testCase.ToArray()
                    };
                }
            }

            yield return new Test
            {
                Environment = testTarget.Environment,
                Application = testTarget.Application,
                Url = definition.Parameters.Any()? request.GetLinkWithQuery(testTarget, definition, definition.Parameters) : request.GetLink(testTarget, definition),
                Tags = definition.Tags,
                Parameters = definition.Parameters.Any()
                    ? definition.Parameters.ToArray()
                    : Array.Empty<Parameter>()
            };
        }
    }
}