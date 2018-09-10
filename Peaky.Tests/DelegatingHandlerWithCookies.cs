// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Peaky.Tests
{
    internal class DelegatingHandlerWithCookies : DelegatingHandler
    {
        private readonly CookieContainer cookieContainer = new CookieContainer();

        internal DelegatingHandlerWithCookies(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            AddCookiesTo(request);

            var response = await base.SendAsync(request, cancellationToken);

            SetCookiesOn(response);

            return response;
        }

        private void AddCookiesTo(HttpRequestMessage request)
        {
            var cookieHeader = cookieContainer.GetCookieHeader(request.RequestUri);

            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
            }
        }

        private void SetCookiesOn(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                foreach (var cookie in cookies)
                {
                    cookieContainer.SetCookies(
                        response.RequestMessage.RequestUri,
                        cookie);
                }
            }
        }
    }
}
