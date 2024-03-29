// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;

namespace Peaky;

internal class AnonymousMethodInfo<T> : AnonymousMethodInfo
{
    public AnonymousMethodInfo(Func<T> anonymousMethod) : base(anonymousMethod.GetMethodInfo())
    {
    }
}