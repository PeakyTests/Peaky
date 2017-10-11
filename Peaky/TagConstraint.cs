// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using Its.Recipes;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Peaky
{
    internal class TagConstraint : TestConstraint
    {
        private string[] _tags;

        public TagConstraint(TestDefinition testDefinition)
        {
            if (typeof(IHaveTags).IsAssignableFrom(testDefinition.TestType))
            {
                TestDefinition = testDefinition;
            }
            else
            {
                _tags = new string[0];
            }
        }

        protected override bool Match(TestTarget target, HttpRequest request)
        {
            _tags = _tags ?? (_tags = target.ResolveDependency(TestDefinition.TestType)
                                            .IfTypeIs<IHaveTags>()
                                            .Then(test =>
                                            {
                                                var tags = test.Tags;
                                                TestDefinition.Tags = tags;
                                                return tags;
                                            })
                                            .ElseDefault()
                                            .OrEmpty()
                                            .ToArray());

            return DoTestsMatchFilterRequest(_tags, request);
        }

        private static bool DoTestsMatchFilterRequest(string[] testTags, HttpRequest request)
        {
            //If no tags were requested, then then it is a match
            if (!request.Query.Any())
            {
                return true;
            }

            var includeTags = request.Query
                                     .Where(t => t.Value.Contains("true", StringComparer.OrdinalIgnoreCase))
                                     .Select(t => t.Key)
                                     .ToArray();
            var excludeTags = request.Query.Where(t => t.Value.Contains("false", StringComparer.OrdinalIgnoreCase))
                                     .Select(t => t.Key)
                                     .ToArray();

            return !excludeTags.Intersect(testTags, StringComparer.OrdinalIgnoreCase).Any() &&
                   includeTags.Intersect(testTags, StringComparer.OrdinalIgnoreCase).Count() == includeTags.Length;
        }
    }
}