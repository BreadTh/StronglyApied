
using System;

namespace BreadTh.StronglyApied.Tests.Tools
{
    public struct AttributeSignature
    {
        public Type attributeType;
        public object[] parameters;
        public AttributeSignature(Type attributeType, params object[] parameters) 
        {
            this.attributeType = attributeType;
            this.parameters = parameters;
        }
    }
}