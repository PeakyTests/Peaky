// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Peaky;

public class AggregationAssertionException<TState> : TestFailedException
{
    public AggregationAssertionException(string message, TState state)
        : base(message)
    {
        State = state;
    }

    public TState State { get; }
}