using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using FsCheck;

using BreadTh.StronglyApied.Attributes;

namespace BreadTh.StronglyApied.Tests.Tools
{
    public class ClassBuilder
    {
        TypeBuilder _typeBuilder;

        public ClassBuilder(DataModel dataModel) : this()
        {
            _typeBuilder.SetCustomAttribute(
                new CustomAttributeBuilder(
                    typeof(StronglyApiedRootAttribute).GetConstructor(new Type[]{ typeof(DataModel) })
                ,   new object[] { dataModel }));
        }

        private ClassBuilder() 
        {
            string typeSignature = Guid.NewGuid().ToString();

            _typeBuilder = 
                    AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(typeSignature), AssemblyBuilderAccess.Run)
                    .DefineDynamicModule("MainModule")
                    .DefineType(
                        typeSignature
                    ,       TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass 
                        |   TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout
                    ,   null);

            _typeBuilder.DefineDefaultConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
                
        }

        public ClassBuilder AddValueField<FIELD_TYPE>(string fieldName, Type attributeType, params object[] attributeParameters) =>
            AddValueField(fieldName, typeof(FIELD_TYPE), attributeType, attributeParameters);

        public ClassBuilder AddValueField(string fieldName, Type fieldType, Type attributeType, params object[] attributeParameters) 
        {
            FieldBuilder fieldBuilder = _typeBuilder.DefineField(fieldName, fieldType, FieldAttributes.Public);
            AddAttributeToField(fieldBuilder, attributeType, attributeParameters);
            return this;
        }

        public ClassBuilder AddValueField<FIELD_TYPE, ATTRIBUTE_TYPE>(string fieldName, params object[] attributeParameters) =>
            AddValueField<FIELD_TYPE>(fieldName, typeof(ATTRIBUTE_TYPE), attributeParameters);

        public ClassBuilder AddValueField<FIELD_TYPE>(string fieldName, params AttributeSignature[] attributeParameters) 
        {
            FieldBuilder fieldBuilder = _typeBuilder.DefineField(fieldName, typeof(FIELD_TYPE), FieldAttributes.Public);
            foreach(AttributeSignature att in attributeParameters)
                AddAttributeToField(fieldBuilder, att.attributeType, att.parameters);

            return this;
        }

        public Type Create() =>
            _typeBuilder.CreateType();

        private void AddAttributeToField(FieldBuilder fieldBuilder, Type attributeType, object[] attributeParameters) 
        {
            attributeParameters ??= new object[0];

            fieldBuilder.SetCustomAttribute(
                new CustomAttributeBuilder(attributeType.GetConstructor(
                        attributeParameters.Select(x => x.GetType()).ToArray())
                ,   attributeParameters));
        }

        public ClassBuilder AddClassField(string fieldName, bool optional, Func<ClassBuilder, Type> innerClassBuilderDelegate) 
        {
            AddValueField(fieldName, innerClassBuilderDelegate(new ClassBuilder()), typeof(StronglyApiedObjectAttribute), optional);
            return this;
        }
    }
}