// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Its.Recipes;
using Microsoft.AspNetCore.Mvc;

namespace Peaky
{
    [TestUiHtmlConfiguration]
    public class PeakyTestController : Controller
    {
        private readonly TestTargetRegistry targets;
        private readonly IDictionary<string, TestDefinition> testDefinitions;

        public PeakyTestController(TestTargetRegistry testTargets, IDictionary<string, TestDefinition> testDefinitions)
        {
            this.testDefinitions = testDefinitions ?? throw new ArgumentNullException(nameof(testDefinitions));
            this.targets = testTargets ?? throw new ArgumentNullException(nameof(testTargets));
        }

        [HttpGet]
        public dynamic Tests(string environment = null, string application = null)
        {
            if (environment != null &&
                !targets.Any(tt => tt.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound();
            }

            if (application != null &&
                !targets.Any(tt => tt.Application.Equals(application, StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound();
            }

            var environments = targets
                .Where(tt => environment == null ||
                             tt.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase))
                .Where(tt => application == null ||
                             tt.Application.Equals(application, StringComparison.OrdinalIgnoreCase))
                .Select(tt => new
                {
                    tt.Application,
                    tt.Environment
                })
                .ToArray();

            return new
            {
                Tests = testDefinitions
                    .Select(t => t.Value)
                    .SelectMany(t =>
                                    environments.Select(ea =>
                                                            new Test
                                                            {
                                                                Environment = ea.Environment,
                                                                Application = ea.Application,
                                                                Url = Url.Link(t.RouteName,
                                                                               new
                                                                               {
                                                                                   ea.Application,
                                                                                   ea.Environment
                                                                               }),
                                                                Tags = t.Tags,
                                                                Parameters = t.Parameters.Any() ? t.Parameters.ToArray() : null
                                                            })
                                                .Where(l => l.Url != null))
                    .OrderBy(t => t.Url.ToString())
            };
        }

        [HttpGet]
        [HttpPost]
        [TracingFilter]
        public async Task<dynamic> Run(string environment, string application, string testName)
        {
            var target = targets.Get(environment, application);

            var result = testDefinitions[testName].Run(ControllerContext, target.ResolveDependency);

            if (result is Task)
            {
                if (result.GetType() == typeof(Task) ||
                    result.GetType().ToString() == "System.Threading.Tasks.Task`1[System.Threading.Tasks.VoidTaskResult]")
                {
                    await result;
                    return Task.FromResult(Unit.Default);
                }

                return await result;
            }

            return result;
        }
    }
}