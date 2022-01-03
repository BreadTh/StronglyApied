using System;
using System.Collections.Generic;

namespace BreadTh.StronglyApied.Core
{
    public enum MemberTypeCategory { Array, Object, Value }

    public abstract class ModelMapperBase
    {
        public abstract (object result, List<ErrorDescription> errors) MapModel(string rawbody, Type rootType);

        protected static MemberTypeCategory DetermineMemberTypeCategory(Type type) 
        {
            ThrowIfTypeUnsupported(type);

            if (type.IsArray)
                return MemberTypeCategory.Array;

            if (type.IsObject())
                return MemberTypeCategory.Object;

            return MemberTypeCategory.Value;
        }

        protected static void ThrowIfTypeUnsupported(Type type)
        {
            if ( 
                type.IsGenericType 
            &&  (   type.GetGenericTypeDefinition() == typeof(IEnumerable<>) 
                ||  type.GetGenericTypeDefinition() == typeof(IList<>)
                ||  type.GetGenericTypeDefinition() == typeof(List<>)
                )
            )
                throw new NotImplementedException("Generic lists are not yet supported. Use T[] instead.");

            if (type.IsStruct())
                throw new NotImplementedException("Structs are not yet supported. Use classes instead.");

            if (type.IsArray && !type.IsSZArray && type.GetElementType() != typeof(IList<>))
                throw new NotImplementedException("Only single-dimensional arrays are currently supported.");
        }
    }
}
