using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Xml.Linq;
using ValueOf;

namespace BreadTh.StronglyApied.Core
{
    public static class ExtensionMethods
    {
        public static bool IsStruct(this Type type)
        {
            if(type.IsGenericType && type.GetGenericTypeDefinition().ToString() == "System.Nullable`1[T]")
                return IsStruct(type.GetGenericArguments()[0]);

            return
                type.IsValueType
            && !type.IsPrimitive
            && type != typeof(decimal)  //decimal is the only builtin type that's both valuetype and not a primitive.
            && type != typeof(DateTime) //though DateTime is also both - but we want to treat it as a value.
            && type != typeof(Guid)     //Ditto.
            && !type.IsEnum;
        }


        public static bool IsObject(this Type type)
        {
            if(new List<Type>{ typeof(string), typeof(MailAddress) }.Contains(type))
                return false;

            //we want to treat ValueOf as a field, not as an object.
            if(type.BaseType != null && type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(ValueOf<,>))
                return false;

            return type.IsClass;
        }

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
