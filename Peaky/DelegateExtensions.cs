// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Peaky
{
    internal static class DelegateExtensions
    {
        private static readonly ConcurrentDictionary<MethodInfo, AnonymousMethodInfo> anonymousMethodInfos =
            new ConcurrentDictionary<MethodInfo, AnonymousMethodInfo>();

        public static AnonymousMethodInfo GetAnonymousMethodInfo<T>(this Func<T> anonymousMethod) =>
            anonymousMethodInfos.GetOrAdd(anonymousMethod.GetMethodInfo(), m => new AnonymousMethodInfo<T>(anonymousMethod));
    }
}
