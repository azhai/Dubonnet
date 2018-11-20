using System;

namespace Dubonnet.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDescAttribute : Attribute
    {
        public readonly string Description;

        public EnumDescAttribute(string desc)
        {
            Description = desc;
        }
    }

    public static class EnumExtensions
    {
        public static string GetDesc(this Enum enumValue)
        {
            var enumStr = enumValue.ToString();
            var info = enumValue.GetType().GetField(enumStr);
            var attrs = info.GetCustomAttributes(typeof(EnumDescAttribute), false);
            if (attrs.Length == 0) return enumStr;
            var attr = (EnumDescAttribute) attrs[0];
            return attr?.Description ?? enumStr;
        }
    }
}
