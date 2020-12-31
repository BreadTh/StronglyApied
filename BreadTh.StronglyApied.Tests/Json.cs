using System;
using System.Collections.Generic;
using System.Net.Mail;

using FsCheck;
using FsCheck.Xunit;
using Newtonsoft.Json.Linq;

using BreadTh.StronglyApied.Attributes;
using BreadTh.StronglyApied.Tests.Tools;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Linq;

namespace BreadTh.StronglyApied.Tests
{
    public class Json
    {
        IModelValidator _validator = new ModelValidator();

        [StronglyApiedRoot(DataModel.Json)] public class ReturnsErrorOnInvalidJsonModel { }

        [Property(Arbitrary = new[] { typeof(InvalidJsonObjectGenerator) })]
        public Property ReturnsErrorOnInvalidJson(string input)
        {
            (ReturnsErrorOnInvalidJsonModel result, List<ErrorDescription> errors) = 
                _validator.TryParse<ReturnsErrorOnInvalidJsonModel>(input);

            return (
                result == null
            &&  errors.Count == 1
            &&  errors[0].id == "34877d2e-0014-4f6a-a9d7-1b9bdf63a502"
            ).ToProperty();            
        }

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property BoolField(string fieldName, bool value) =>
            ParseFieldInRoot<bool, StronglyApiedBoolAttribute>(fieldName, value, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property IntField(string fieldName, int value) =>
            ParseFieldInRoot<int, StronglyApiedIntAttribute>(fieldName, value, int.MinValue, int.MaxValue, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property LongField(string fieldName, long value) =>
            ParseFieldInRoot<long, StronglyApiedLongAttribute>(fieldName, value, long.MinValue, long.MaxValue, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property DecimalField(string fieldName, decimal value) =>
            ParseFieldInRoot<decimal, StronglyApiedDecimalAttribute>(
                fieldName, value, "-79228162514264337593543950335", "79228162514264337593543950335", 0, 29, false); 

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property StringField(string fieldName, string value) =>
            ParseFieldInRoot<string, StronglyApiedStringAttribute>(fieldName, value, 0, int.MaxValue, false);

        [JsonConverter(typeof(StringEnumConverter))] public enum TestEnum { Abc, Def, Hij }
        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property OptionField(string fieldName, TestEnum value) =>
            ParseFieldInRoot<TestEnum, StronglyApiedOptionAttribute>(fieldName, value, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property EmailAddressField(string fieldName, MailAddress value) =>
            ParseFieldInRoot<string, StronglyApiedEmailAddressAttribute>(fieldName, value.Address, false);

        private Property ParseFieldInRoot<VALUE_TYPE, FIELD_ATTRIBUTE_TYPE>(
            string fieldName
        ,   VALUE_TYPE value
        ,   params object[] fieldAttributeValues)
        {
            Type type = new ClassBuilder(DataModel.Json)
                .AddValueField<VALUE_TYPE, FIELD_ATTRIBUTE_TYPE>(fieldName, fieldAttributeValues)
                .Create();

            string input = new JObject(){ { fieldName, JToken.FromObject(value) } }.ToString();

            (object result, List<ErrorDescription> errors) = 
                _validator.TryParse(input, type);

            return (
                errors.Count == 0 
            &&  value.Equals((VALUE_TYPE)type.GetField(fieldName).GetValue(result))
            ).ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property BoolArrayField(string fieldName, bool[] values) =>
            ParseValuesInArrayInField<bool, StronglyApiedBoolAttribute>(fieldName, values, false);
        
        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property IntArrayField(string fieldName, int[] values) =>
            ParseValuesInArrayInField<int, StronglyApiedIntAttribute>(fieldName, values, int.MinValue, int.MaxValue, false);
        
        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })]
        public Property LongArrayField(string fieldName, long[] values) =>
            ParseValuesInArrayInField<long, StronglyApiedLongAttribute>(fieldName, values, long.MinValue, long.MaxValue, false);
        
        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property DecimalArrayField(string fieldName, decimal[] values) =>
            ParseValuesInArrayInField<decimal, StronglyApiedDecimalAttribute>(
                fieldName, values, "-79228162514264337593543950335", "79228162514264337593543950335", 0, 29, false);
        
        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property StringArrayField(string fieldName, string[] values) =>
            ParseValuesInArrayInField<string, StronglyApiedStringAttribute>(fieldName, values, 0, int.MaxValue, false);
        
        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })]
        public Property OptionArrayField(string fieldName, TestEnum[] values) =>
            ParseValuesInArrayInField<TestEnum, StronglyApiedOptionAttribute>(fieldName, values, false);
        
        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property EmailAddressArrayField(string fieldName, MailAddress[] values) =>
            ParseValuesInArrayInField<string, StronglyApiedEmailAddressAttribute>(fieldName, values.Select(value => value.Address).ToArray(), false);

        private Property ParseValuesInArrayInField<VALUE_TYPE, FIELD_ATTRIBUTE_TYPE>(string fieldName, VALUE_TYPE[] values, params object[] fieldAttributeValues)
        {
            Type type = new ClassBuilder(DataModel.Json)
                .AddValueField<VALUE_TYPE[]>(
                    fieldName
                ,   new AttributeSignature[]
                    {   new AttributeSignature(typeof(StronglyApiedArrayAttribute), 0, int.MaxValue, false)
                    ,   new AttributeSignature(typeof(FIELD_ATTRIBUTE_TYPE), fieldAttributeValues)
                    })
                .Create();
            
            string input = new JObject(){ { fieldName, JArray.FromObject(values.Select(value => JToken.FromObject(value))) } }.ToString();

            (object result, List<ErrorDescription> errors) = 
                _validator.TryParse(input, type);

            Func<bool> test = () =>
            {
                if(errors.Count != 0)
                    return false;

                VALUE_TYPE[] array = (VALUE_TYPE[])type.GetField(fieldName).GetValue(result);
                for (int index = values.Length - 1; index >= 0; --index)
                    if(!array[index].Equals(values[index]) )
                        return false;

                return true;
            };
            return test.ToProperty();
        }

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property BoolFieldObjectField(string objectFieldName, string valueFieldName, bool value) =>
            ParseFieldInObject<bool, StronglyApiedBoolAttribute>(objectFieldName, valueFieldName, value, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property IntFieldObjectField(string objectFieldName, string valueFieldName, int value) =>
            ParseFieldInObject<int, StronglyApiedIntAttribute>(
                objectFieldName, valueFieldName, value, int.MinValue, int.MaxValue, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property LongFieldObjectField(string objectFieldName, string valueFieldName, long value) =>
            ParseFieldInObject<long, StronglyApiedLongAttribute>(
                objectFieldName, valueFieldName, value, long.MinValue, long.MaxValue, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property DecimalFieldObjectField(string objectFieldName, string valueFieldName, decimal value) =>
            ParseFieldInObject<decimal, StronglyApiedDecimalAttribute>(
                objectFieldName, valueFieldName, value, "-79228162514264337593543950335", "79228162514264337593543950335", 0, 29, false); 

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property StringFieldObjectField(string objectFieldName, string valueFieldName, string value) =>
            ParseFieldInObject<string, StronglyApiedStringAttribute>(objectFieldName, valueFieldName, value, 0, int.MaxValue, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property OptionFieldObjectField(string objectFieldName, string valueFieldName, TestEnum value) =>
            ParseFieldInObject<TestEnum, StronglyApiedOptionAttribute>(objectFieldName, valueFieldName, value, false);

        [Property(Arbitrary = new[] { typeof(CSharpFieldNameGenerator) })] 
        public Property EmailAddressFieldObjectField(string objectFieldName, string valueFieldName, MailAddress value) =>
            ParseFieldInObject<string, StronglyApiedEmailAddressAttribute>(objectFieldName, valueFieldName, value.Address, false);

        private Property ParseFieldInObject<VALUE_TYPE, FIELD_ATTRIBUTE_TYPE>(
            string objectFieldName
        ,   string valueFieldName
        ,   VALUE_TYPE value
        ,   params object[] fieldAttributeValues)
        {
            Type innerType = default;
            Type outerType = new ClassBuilder(DataModel.Json)
                .AddClassField(objectFieldName, false, (ClassBuilder innerBuilder) => 
                {
                    innerType = innerBuilder
                        .AddValueField<VALUE_TYPE, FIELD_ATTRIBUTE_TYPE>(valueFieldName, fieldAttributeValues)
                        .Create();
                    return innerType;
                })
                .Create();

            string input = new JObject(){{ objectFieldName, new JObject(){{ valueFieldName, JToken.FromObject(value) }} }}.ToString();

            (object result, List<ErrorDescription> errors) = 
                _validator.TryParse(input, outerType);

            Func<bool> test = () =>
            {
                if(errors.Count != 0)
                    return false;

                return innerType
                    .GetField(valueFieldName).GetValue(
                        outerType.GetField(objectFieldName).GetValue(result)
                    ).Equals(value);
            };
            return test.ToProperty();
        }

        /*
        multiple
        empty
        */
    }
}