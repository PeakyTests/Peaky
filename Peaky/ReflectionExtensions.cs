using System;
using System.Linq;
using System.Reflection;

namespace Peaky;

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
        return method.NotDefinedOn(typeof(T));
    }

    public static bool NotDefinedOn(this MethodInfo method, Type @interface)
    {
        if (method == null)
        {
            throw new ArgumentNullException(nameof(method));
        }
        if (method.DeclaringType?.GetInterface(@interface.Name) == null)
        {
            return true;
        }

        var map = method.DeclaringType.GetInterfaceMap(@interface);
        var found = map.TargetMethods.Contains(method);
        return !found;
    }
}