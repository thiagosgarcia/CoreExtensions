using System;
using PenguinSoft.CoreExtensions.Helpers.Enum;

namespace PenguinSoft.CoreExtensions.Extensions
{
    public static class EnumExtensions
    {

        public static string GetDescription(this Enum value)
        {
            return EnumHelper.GetDescription(value);
        }

    }
}