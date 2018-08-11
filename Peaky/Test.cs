// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
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

        public static IEnumerable<Test> CreateTests(TestTarget testTarget, TestDefinition definition)
        {
            return Enumerable.Empty<Test>();
        }
    }
}