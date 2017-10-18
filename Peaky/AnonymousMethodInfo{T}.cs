using System;
using System.Reflection;

namespace Peaky
{
    internal class AnonymousMethodInfo<T> : AnonymousMethodInfo
    {
        public AnonymousMethodInfo(Func<T> anonymousMethod) : base(anonymousMethod.GetMethodInfo())
        {
        }
    }
}