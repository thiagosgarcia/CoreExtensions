using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using PenguinSoft.CoreExtensions.Extensions;

namespace PenguinSoft.CoreExtensions.Helpers.Enum
{
    public static class EnumHelper
    {

        public static string GetDescription(object value)
        {
            var type = value.GetType();
            var name = System.Enum.GetName(type, value);
            if (name == null) return string.Empty;
            var field = type.GetField(name);
            if (field != null)
            {
                var attr = GetDescriptionAttribute(field);
                if (attr != null) return attr.Description;
            }
            return name;
        }

        private static DescriptionAttribute GetDescriptionAttribute(MemberInfo field)
        {
            return
                Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute ??
                Attribute.GetCustomAttribute(field, typeof(LocalizedDescriptionAttribute)) as DescriptionAttribute;
        }

        public static string GetDescription(Type enumType, string value)
        {
            return GetDescription(System.Enum.Parse(enumType, value));
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)System.Enum.Parse(typeof(T), value, true);
        }

        public static IEnumerable<string> EnumToList<TEnum>(bool retrieveDescription = true)
            where TEnum : struct
        {
            var names = System.Enum.GetNames(typeof(TEnum));
            if (!retrieveDescription)
                return names.AsEnumerable();

            var result = names.Select(x =>
            {
                System.Enum.TryParse(x, out TEnum e);
                return (e as System.Enum).GetDescription();
            });

            return result;
        }

    }
}