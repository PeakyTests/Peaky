// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Peaky
{
    [Serializable]
    [Obsolete("Use TestFailedException instead.")]
    public class AssertionFailedException : Exception
    {
        public AssertionFailedException(string error) : base(error)
        {
        }
    }
}