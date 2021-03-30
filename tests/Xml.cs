using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Linq;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using FsCheck;
using FsCheck.Xunit;

using BreadTh.StronglyApied.Attributes;
using BreadTh.StronglyApied.Tests.Tools;

namespace BreadTh.StronglyApied.Tests
{
    public class Xml
    {
        IModelValidator _validator = new ModelValidator();

        public Xml() 
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        [StronglyApiedRoot(DataModel.Xml)] public class ReturnsErrorOnInvalidJXmlodel { }

        [Property(Arbitrary = new[] { typeof(InvalidJsonObjectGenerator) })]
        public Property ReturnsErrorOnInvalidXml(string input)
        {
            (ReturnsErrorOnInvalidJXmlodel result, List<ErrorDescription> errors) = 
                _validator.Parse<ReturnsErrorOnInvalidJXmlodel>(input);

            return (
                result == null
            &&  errors.Count == 1
            &&  errors[0].id == "34877d2e-0014-4f6a-a9d7-1b9bdf63a502"
            ).ToProperty();            
        }

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property BoolElement(string fieldName, bool value) =>
            ParseElementInRoot<bool, StronglyApiedBoolAttribute>(fieldName, value, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property IntElement(string fieldName, int value) =>
            ParseElementInRoot<int, StronglyApiedIntAttribute>(fieldName, value, int.MinValue, int.MaxValue, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property LongElement(string fieldName, long value) =>
            ParseElementInRoot<long, StronglyApiedLongAttribute>(fieldName, value, long.MinValue, long.MaxValue, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property DecimalElement(string fieldName, decimal value) =>
            ParseElementInRoot<decimal, StronglyApiedDecimalAttribute>(
                fieldName, value, "-79228162514264337593543950335", "79228162514264337593543950335", 0, 29, false); 

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property StringElement(string fieldName, string value) =>
            ParseElementInRoot<string, StronglyApiedStringAttribute>(fieldName, value, 0, int.MaxValue, false);

        [JsonConverter(typeof(StringEnumConverter))] public enum TestEnum { Abc, Def, Hij }
        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property OptionElement(string fieldName, TestEnum value) =>
            ParseElementInRoot<TestEnum, StronglyApiedOptionAttribute>(fieldName, value, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property EmailAddressElement(string fieldName, MailAddress value) =>
            ParseElementInRoot<string, StronglyApiedEmailAddressAttribute>(fieldName, value.Address, false);

        private Property ParseElementInRoot<VALUE_TYPE, FIELD_ATTRIBUTE_TYPE>(
            string fieldName
        ,   VALUE_TYPE value
        ,   params object[] fieldAttributeValues)
        {
            Type type = new ClassBuilder(DataModel.Xml)
                .AddValueField<VALUE_TYPE>(fieldName, new AttributeSignature[] 
                {   new AttributeSignature(typeof(FIELD_ATTRIBUTE_TYPE), fieldAttributeValues)
                ,   new AttributeSignature(typeof(StronglyApiedXmlElementAttribute))
                })
                .Create();

            string input = CreateXmlString((XmlWriter x) => 
            {
                x.WriteStartElement(fieldName);
                x.WriteString(value.ToString());
                x.WriteEndElement();
            });

            (object result, List<ErrorDescription> errors) = 
                _validator.Parse(input, type);

            var resVal = (VALUE_TYPE)type.GetField(fieldName).GetValue(result);

            return (
                errors.Count == 0 
            &&  value.Equals(resVal)
            ).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property BoolElementArrayElement(string fieldName, NonEmptyArray<bool> values) =>
            ParseValuesInElementsInArrayInElement<bool, StronglyApiedBoolAttribute>(fieldName, values.Get, false);
        
        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property IntElementArrayElement(string fieldName, NonEmptyArray<int> values) =>
            ParseValuesInElementsInArrayInElement<int, StronglyApiedIntAttribute>(fieldName, values.Get, int.MinValue, int.MaxValue, false);
        
        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })]
        public Property LongElementArrayElement(string fieldName, NonEmptyArray<long> values) =>
            ParseValuesInElementsInArrayInElement<long, StronglyApiedLongAttribute>(fieldName, values.Get, long.MinValue, long.MaxValue, false);
        
        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property DecimalElementArrayElement(string fieldName, NonEmptyArray<decimal> values) =>
            ParseValuesInElementsInArrayInElement<decimal, StronglyApiedDecimalAttribute>(
                fieldName, values.Get, "-79228162514264337593543950335", "79228162514264337593543950335", 0, 29, false);
        
        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property StringElementArrayElement(string fieldName, NonEmptyArray<string> values) =>
            ParseValuesInElementsInArrayInElement<string, StronglyApiedStringAttribute>(fieldName, values.Get, 0, int.MaxValue, false);
        
        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })]
        public Property OptionElementArrayElement(string fieldName, NonEmptyArray<TestEnum> values) =>
            ParseValuesInElementsInArrayInElement<TestEnum, StronglyApiedOptionAttribute>(fieldName, values.Get, false);
        
        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property EmailElementAddressArrayElement(string fieldName, NonEmptyArray<MailAddress> values) =>
            ParseValuesInElementsInArrayInElement<string, StronglyApiedEmailAddressAttribute>(fieldName, values.Get.Select(value => value.Address).ToArray(), false);

        private Property ParseValuesInElementsInArrayInElement<VALUE_TYPE, FIELD_ATTRIBUTE_TYPE>(
            string fieldName
        ,   VALUE_TYPE[] values
        ,   params object[] fieldAttributeValues)
        {
            Type innerType = default;
            Type outerType = new ClassBuilder(DataModel.Xml)
                .AddClassField("root", false, x => 
                {
                    x.AddValueField<VALUE_TYPE[]>(
                        fieldName
                    ,   new AttributeSignature[]
                        {   new AttributeSignature(typeof(StronglyApiedArrayAttribute), 0, int.MaxValue, false)
                        ,   new AttributeSignature(typeof(FIELD_ATTRIBUTE_TYPE), fieldAttributeValues)
                        });
                    innerType = x.Create();
                    return innerType;
                })
                .Create();
            
            string input = CreateXmlString((XmlWriter x) => 
            {
                x.WriteStartElement("root");
                foreach(var value in values) 
                {
                    x.WriteStartElement(fieldName);
                    x.WriteString(value.ToString());
                    x.WriteEndElement();
                }
                x.WriteEndElement();
            });

            (object result, List<ErrorDescription> errors) = 
                _validator.Parse(input, outerType);

            Func<bool> test = () =>
            {
                if(errors.Count != 0)
                    return false;

                VALUE_TYPE[] array = (VALUE_TYPE[])innerType.GetField(fieldName).GetValue(outerType.GetField("root").GetValue(result));
                for (int index = values.Length - 1; index >= 0; --index)
                    if(!array[index].Equals(values[index]) )
                        return false;

                return true;
            };
            return test.ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property BoolAttributeElement(string objectFieldName, string valueFieldName, bool value) =>
            ParseAttributeInElement<bool, StronglyApiedBoolAttribute>(objectFieldName, valueFieldName, value, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property IntAttributeElement(string objectFieldName, string valueFieldName, int value) =>
            ParseAttributeInElement<int, StronglyApiedIntAttribute>(
                objectFieldName, valueFieldName, value, int.MinValue, int.MaxValue, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property LongAttributeElement(string objectFieldName, string valueFieldName, long value) =>
            ParseAttributeInElement<long, StronglyApiedLongAttribute>(
                objectFieldName, valueFieldName, value, long.MinValue, long.MaxValue, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property DecimalAttributeElement(string objectFieldName, string valueFieldName, decimal value) =>
            ParseAttributeInElement<decimal, StronglyApiedDecimalAttribute>(
                objectFieldName, valueFieldName, value, "-79228162514264337593543950335", "79228162514264337593543950335", 0, 29, false); 

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property StringAttributeElement(string objectFieldName, string valueFieldName, string value) =>
            ParseAttributeInElement<string, StronglyApiedStringAttribute>(objectFieldName, valueFieldName, value, 0, int.MaxValue, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property OptionAttributeElement(string objectFieldName, string valueFieldName, TestEnum value) =>
            ParseAttributeInElement<TestEnum, StronglyApiedOptionAttribute>(objectFieldName, valueFieldName, value, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property EmailAddressAttributeElement(string objectFieldName, string valueFieldName, MailAddress value) =>
            ParseAttributeInElement<string, StronglyApiedEmailAddressAttribute>(objectFieldName, valueFieldName, value.Address, false);

        private Property ParseAttributeInElement<VALUE_TYPE, FIELD_ATTRIBUTE_TYPE>(
            string elementName
        ,   string attributeName
        ,   VALUE_TYPE value
        ,   params object[] fieldAttributeValues)
        {
            Type innerType = default;
            Type outerType = new ClassBuilder(DataModel.Xml)
                .AddClassField(elementName, false, (ClassBuilder innerBuilder) => 
                {
                    innerType = innerBuilder
                        .AddValueField<VALUE_TYPE>(attributeName, new AttributeSignature[]
                        {   new AttributeSignature() { attributeType = typeof(FIELD_ATTRIBUTE_TYPE), parameters = fieldAttributeValues }
                        ,   new AttributeSignature() { attributeType = typeof(StronglyApiedXmlAttributeAttribute)}
                        })
                        .Create();
                    return innerType;
                })
                .Create();

            string input = CreateXmlString((XmlWriter x) => 
            {
                x.WriteStartElement(elementName);  
                x.WriteAttributeString(attributeName, value.ToString());
                x.WriteEndElement();
            });

            (object result, List<ErrorDescription> errors) = 
                _validator.Parse(input, outerType);

            Func<bool> test = () =>
            {
                if(errors.Count != 0)
                    return false;

                return innerType
                    .GetField(attributeName).GetValue(
                        outerType.GetField(elementName).GetValue(result)
                    ).Equals(value);
            };
            return test.ToProperty();
        }

        private string CreateXmlString(Action<XmlWriter> xmlDelegate) 
        {
            using StringWriter stringWriter = new StringWriter();
            using XmlWriter xmlWriter = XmlWriter.Create(stringWriter);
            xmlDelegate(xmlWriter);
            xmlWriter.Flush();
            return stringWriter.ToString();
        }
    }
}