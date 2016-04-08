// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
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
            catch (Exception e)
            {
                Console.WriteLine("Invalid Json: " + json);
                throw;
            }
        }
    }
}