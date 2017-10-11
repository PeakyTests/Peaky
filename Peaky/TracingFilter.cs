// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Peaky
{
    public class TracingFilter : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
          //  TraceBuffer.Initialize();

            await base.OnActionExecutionAsync(context, next);

//            TraceBuffer.Clear();

            var buffer = new TraceBuffer();

            if (actionExecutedContext.Exception == null)
            {
                if (context.HttpContext.Response.StatusCode == 200)
                {
                    string originalContent ="";

                    context.Result = new OkObjectResult($"{buffer}\n\n{originalContent}".Trim());
                }
                else
                {
                    //originalContent = context.Exception.ToLogString();
                }
            }
        }

//        internal class TraceListener : System.Diagnostics.TraceListener
//        {
//            public static readonly TraceListener Instance = new TraceListener();
//
//            public override void Write(string message) => WriteLine(message);
//
//            public override void WriteLine(string message)
//            {
//                var buffer = TraceBuffer.Current;
//
//                buffer?.Write(message);
//            }
//        }
    }
}
