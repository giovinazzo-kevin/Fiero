using System;
using System.ComponentModel;
using System.Reflection;

namespace Fiero.Business
{
    public static class DescriptionExtensions
    {
        public static string Describe(this Enum enumVal)
        {
            if (enumVal.GetType().GetMember(enumVal.ToString())[0].GetCustomAttribute<DescriptionAttribute>() is { } attr)
            {
                return attr.Description;
            }
            return enumVal.ToString();
        }
    }
}
