// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Peaky.Tests
{
    public static class JsonSerializationExtensions
    {
        public static dynamic JsonContent(this HttpResponseMessage response)
        {
            var json = response.Content.ReadAsStringAsync().Result;
            try
            {
                return JToken.Parse(json);
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid Json: " + json);
                throw;
            }
        }

        internal static async Task<TestList> AsTestList(this HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TestList>(json);
        }

        internal static async Task<TestResult> AsTestResult(this HttpResponseMessage response)
        {
            var content = response.Content;

            var json = await content.ReadAsStringAsync();

            Console.WriteLine();
            Console.WriteLine(json);
            Console.WriteLine();

            return JsonConvert.DeserializeObject<TestResult>(json);
        }
    }
}
