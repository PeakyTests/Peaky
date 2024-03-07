// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Peaky;

public interface IHaveTags : IPeakyTest
{
    string[] Tags { get; }
}