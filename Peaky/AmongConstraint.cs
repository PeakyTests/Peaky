// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Peaky
{
    public class AmongConstraint : IRouteConstraint
    {
        public readonly string[] AllowedValues;

        public AmongConstraint(string value)
        {
            AllowedValues = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public bool Match(HttpContext httpContext, 
                          IRouter route, 
                          string routeKey, 
                          RouteValueDictionary values, 
                          RouteDirection routeDirection)
        {
            if (values.TryGetValue(routeKey, out var value) && value != null)
            {
                return AllowedValues.Any(allowed => string.Equals(allowed,
                                                                  value.ToString(),
                                                                  StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }
    }
}