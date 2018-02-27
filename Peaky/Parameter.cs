// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using static System.String;

namespace Peaky
{
    internal class Parameter
    {
        public Parameter(string name, object defaultValue)
        {
            if (IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Argument is null or whitespace", nameof(name));
            }

            Name = name;
            DefaultValue = defaultValue;
        }

        public string Name { get; }

        public object DefaultValue { get; }
    }
}
