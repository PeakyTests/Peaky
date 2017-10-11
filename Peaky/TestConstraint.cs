// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Its.Recipes;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Peaky
{
    internal abstract class TestConstraint : IRouteConstraint
    {
        public TestDefinition TestDefinition { get; set; }

        protected abstract bool Match(TestTarget target, HttpRequest request);

        //        public bool Match(HttpRequestMessage request,
        //                          IHttpRoute route,
        //                          string parameterName,
        //                          IDictionary<string, object> values,
        //                          HttpRouteDirection routeDirection)
        //        {
        //            var application = values.IfContains("application").And().IfTypeIs<string>().ElseDefault();
        //            var environment = values.IfContains("environment").And().IfTypeIs<string>().ElseDefault();
        //
        //            var target = request.Properties
        //                                .IfContains("MS_RequestContext")
        //                                .And()
        //                                .IfTypeIs<HttpRequestContext>()
        //                                .Then(config => config.Configuration.TestTargets().TryGet(environment, application))
        //                                .ElseDefault();
        //
        //            return target != null &&
        //                   Match(target, request);
        //        }

        public bool Match(
            HttpContext httpContext,
            IRouter route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            var application = values.IfContains("application").And().IfTypeIs<string>().ElseDefault();
            var environment = values.IfContains("environment").And().IfTypeIs<string>().ElseDefault();

            TestTarget target  = null; //= httpContext.
//                .IfContains("MS_RequestContext")
//                                    .And()
//                                    .IfTypeIs<HttpRequestContext>()
//                                    .Then(config => config.Configuration.TestTargets().TryGet(environment, application))
//                                    .ElseDefault();

            return target != null &&
                   Match(target, httpContext.Request);
        }
    }
}