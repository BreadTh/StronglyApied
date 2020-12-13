using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace BreadTh.StronglyApied.Direct.Core
{
    internal static class ExtensionMethods 
    {
        public static bool IsStruct(this Type type) =>
            type.IsValueType && !type.IsPrimitive && !type.IsEnum;

        public static bool IsNonStringClass(this Type type) =>
            type.IsClass && type != typeof(string);

        public static string ToCultureInvariantString(this JToken token) =>
            token.GetType() == typeof(JValue)
            ?   ((JValue) token).ToString(CultureInfo.InvariantCulture)
            :   token.ToString();
    }
}
