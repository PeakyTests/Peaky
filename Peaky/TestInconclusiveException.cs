// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Peaky
{
    public class TestInconclusiveException : Exception
    {
        public TestInconclusiveException()
        {
        }

        public TestInconclusiveException(string message) : base(message)
        {

        }

        public TestInconclusiveException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}