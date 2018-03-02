namespace ForgelightUnity.Editor.Utils
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class AttributeExtensions
    {
        public static T GetAttribute<T>(this MemberInfo info) where T : Attribute
        {
            return (T)info.GetCustomAttributes(typeof(T), true).FirstOrDefault();
        }

        public static bool HasAttribute<T>(this MemberInfo info) where T : Attribute
        {
            return info.GetAttribute<T>() != null;
        }
    }
}