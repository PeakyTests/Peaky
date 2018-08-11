using System;
using System.Linq;
using System.Reflection;

namespace Peaky
{
    internal static class ReflectionExtensions
    {
        public static object GetDefaultValue(this ParameterInfo parameter)
        {
            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }

            return parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null;
        }

        public static bool NotDefinedOn<T>(this MethodInfo method)
        {
            if (method.DeclaringType.GetInterface(typeof(T).Name) == null)
            {
                return true;
            }

            var map = method.DeclaringType.GetInterfaceMap(typeof(T));
            var found = map.TargetMethods.Contains(method);
            return !found;
        }
    }
}