// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Pocket;
using static Pocket.Logger<Peaky.TestRouter>;

namespace Peaky
{
    internal class TestRouter : PeakyRouter
    {
        private readonly TestTargetRegistry testTargets;
        private readonly TestDefinitionRegistry testDefinitions;
        private readonly string pathBase;

        public TestRouter(
            TestTargetRegistry testTargets,
            TestDefinitionRegistry testDefinitions,
            string pathBase = "/tests") : base(pathBase)
        {
            this.pathBase = pathBase ??
                            throw new ArgumentNullException(nameof(pathBase));

            this.testDefinitions = testDefinitions ??
                                   throw new ArgumentNullException(nameof(testDefinitions));
            this.testTargets = testTargets ??
                               throw new ArgumentNullException(nameof(testTargets));
        }

        public override async Task RouteAsync(RouteContext context)
        {
            var segments = context.HttpContext
                                  .Request
                                  .Path
                                  .Value
                                  .Substring(pathBase.Length)
                                  .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            switch (segments.Length)
            {
                case 0:
                case 1:
                case 2:
                    ListTests(
                        segments.ElementAtOrDefault(0),
                        segments.ElementAtOrDefault(1),
                        context);
                    break;

                case 3:
                    await RunTest(
                        segments[0],
                        segments[1],
                        segments[2],
                        context);
                    break;
            }
        }

        private void ListTests(
            string environment,
            string application,
            RouteContext context)
        {
            using (Log.OnEnterAndExit())
            {
                if (environment != null)
                {
                    if (!testTargets.Any(tt => tt.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase)))
                    {
                        return;
                    }
                }

                if (application != null &&
                    !testTargets.Any(tt => tt.Application.Equals(application, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                context.Handler = async httpContext =>
                {
                    var applicableTargets = testTargets
                        .Where(
                            tt => environment == null ||
                                  tt.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase))
                        .Where(
                            tt => application == null ||
                                  tt.Application.Equals(application, StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                    var tests = testDefinitions
                        .SelectMany(
                            definition =>
                                applicableTargets
                                    .Where(definition.AppliesTo)
                                    .Where(_ =>
                                               MatchesFilter(
                                                   definition.Tags,
                                                   context.HttpContext.Request.Query))
                                    .Select(
                                        target =>
                                            new Test
                                            {
                                                Environment = target.Environment,
                                                Application = target.Application,
                                                Url = context.HttpContext.Request.GetLink(target, definition),
                                                Tags = definition.Tags,
                                                Parameters = definition.Parameters.Any()
                                                                 ? definition.Parameters.ToArray()
                                                                 : null
                                            })
                                    .Where(l => l.Url != null))
                        .OrderBy(t => t.Url.ToString());

                    var json = JsonConvert.SerializeObject(new { Tests = tests });

                    await httpContext.Response.WriteAsync(json);
                };
            }
        }

        private async Task RunTest(
            string environment,
            string application,
            string testName,
            RouteContext context)
        {
            using (Log.OnEnterAndExit())
            {
                TestTarget target;

                try
                {
                    target = testTargets.Get(environment, application);
                }
                catch (TestNotDefinedException)
                {
                    context.Handler = async httpContext => httpContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
                    return;
                }

                var testDefinition = testDefinitions.Get(testName);

                if (!testDefinition.AppliesTo(target))
                {
                    return;
                }

                context.Handler = async httpContext =>
                {
                    TestResult result;

                    try
                    {
                        TraceBuffer.Initialize();

                        var container = target.DependencyRegistry.Container;
                        var returnValue = await testDefinition.Run(
                                              httpContext,
                                              container.Resolve);

                        if (returnValue is Task task)
                        {
                            await task;

                            if (task.GetType().IsGenericType)
                            {
                                var genericTypeParameter = task.GetType().GenericTypeArguments.First();

                                if (genericTypeParameter.IsPublic)
                                {
                                    // task is Task<T> so await to get its return value
                                    returnValue = await (dynamic) task;
                                }
                                else
                                {
                                    returnValue = null;
                                }
                            }
                        }

                        result = TestResult.Pass(returnValue);
                    }
                    catch (ParameterFormatException exception)
                    {
                        result = TestResult.Fail(exception);
                        httpContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                    }
                    catch (Exception exception)
                    {
                        result = TestResult.Fail(exception);
                        httpContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    }

                    var json = JsonConvert.SerializeObject(result);

                    await httpContext.Response.WriteAsync(json);
                };
            }
        }

        private static bool MatchesFilter(
            string[] testTags,
            IQueryCollection query)
        {
            //If no tags were requested, then then it is a match
            if (!query.Any())
            {
                return true;
            }

            var includeTags = query.Where(t => string.Equals(
                                              t.Value.FirstOrDefault(),
                                              "true",
                                              StringComparison.OrdinalIgnoreCase))
                                   .Select(t => t.Key)
                                   .ToArray();

            var excludeTags = query.Where(t =>
                                              string.Equals(t.Value.FirstOrDefault(), "false", StringComparison.OrdinalIgnoreCase))
                                   .Select(t => t.Key)
                                   .ToArray();

            return !excludeTags.Intersect(testTags, StringComparer.OrdinalIgnoreCase).Any() &&
                   includeTags.Intersect(testTags, StringComparer.OrdinalIgnoreCase).Count() == includeTags.Length;
        }
    }
}
