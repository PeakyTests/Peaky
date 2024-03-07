// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Peaky;

public class Percentage : IComparable<Percentage>
{
    private readonly int value;

    public Percentage(int value)
    {
        this.value = value;
    }

    public override string ToString() => $"{value}%";

    public int CompareTo(Percentage other)
    {
        return value.CompareTo(other.value);
    }

    protected bool Equals(Percentage other)
    {
        return value == other.value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((Percentage)obj);
    }

    public override int GetHashCode()
    {
        return value;
    }
}