using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Xml.Linq;

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

        public static bool IsPrimitive(this XElement element)
        {
            if (element.FirstNode == null)
                return true;

            if (element.FirstNode.NextNode != null)
                return false;

            return element.FirstNode.NodeType == System.Xml.XmlNodeType.Text;
        }
    }
}
