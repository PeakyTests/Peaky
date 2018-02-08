// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Peaky
{
    internal static class TypeExtensions
    {
        public static bool IsAnonymous(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.IsDefined(typeof(CompilerGeneratedAttribute), false) &&
                   type.IsGenericType &&
                   type.Name.Contains("AnonymousType") &&
                   (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$")) &&
                   (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        public static bool IsCompilerGenerated(this Type type) =>
            type.IsDefined(typeof(CompilerGeneratedAttribute), false);
    }
}