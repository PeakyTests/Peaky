// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Peaky
{
    public class SuggestRetryException : Exception
    {
        public SuggestRetryException(string message) : base(message)
        {
            
        }
    }
}