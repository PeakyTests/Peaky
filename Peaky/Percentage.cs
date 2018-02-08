// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Peaky
{
    public class Percentage : IComparable<Percentage>
    {
        private readonly int value;

        public Percentage(int value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return string.Format("{0}%", value);
        }

        public int CompareTo(Percentage other)
        {
            return value.CompareTo(other.value);
        }
    }
}