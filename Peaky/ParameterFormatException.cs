using System;

namespace Peaky
{
    internal class ParameterFormatException : FormatException
    {
        public ParameterFormatException(string parameterName, Type parameterType, FormatException e) 
            : base($"The value specified for parameter '{parameterName}' could not be parsed as {parameterType}", e)
        {}
    }
}