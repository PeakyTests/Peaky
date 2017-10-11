// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Peaky
{
    internal class AuthorizeSensorsAttribute : Attribute, IAuthorizationFilter
    {
        private static Func<AuthorizationFilterContext, bool> authorizeRequest = context => false;

        public static Func<AuthorizationFilterContext, bool> AuthorizeRequest
        {
            get => authorizeRequest;
            set => authorizeRequest = value ?? (authorizeRequest = context => false);
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!AuthorizeRequest(context))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}