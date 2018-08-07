// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json;

namespace Peaky
{
    internal class TestInfo
    {
        public string Application { get;  }

        public string Name { get;  }

        public string Environment { get; }

        public Uri Url { get;  }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] Tags { get; }

        public TestInfo(string application,
            string environment,
            string name,
            Uri testUrl,
            string[] testTags = null)
        {
            
            if (string.IsNullOrWhiteSpace(application))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(application));
            }

            if (string.IsNullOrWhiteSpace(environment))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(environment));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            Application = application;
            Environment = environment;
            Tags = testTags;
            Name = name;
            Url = testUrl ?? throw new ArgumentNullException(nameof(testUrl));

        }

    }
}