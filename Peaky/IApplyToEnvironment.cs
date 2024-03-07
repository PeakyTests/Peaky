// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Peaky;

public interface IApplyToEnvironment : IPeakyTest
{
    bool AppliesToEnvironment(string environment);
}