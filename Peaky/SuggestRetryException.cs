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