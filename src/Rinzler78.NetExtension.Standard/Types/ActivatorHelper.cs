using System;

namespace Rinzler78.NetExtension.Types
{
    public static class ActivatorHelper
    {
        public static object CreateInstance(this Type type, params object[] args) => Activator.CreateInstance(type, args);

        public static T CreateInstance<T>(this Type type, params object[] args) => (T)CreateInstance(type, args);
    }
}