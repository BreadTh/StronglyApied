using System;
using BreadTh.StronglyApied.Attributes.Core;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StronglyApiedXmlAttributeAttribute : StronglyApiedXmlRelationBaseAttribute
    {
        public StronglyApiedXmlAttributeAttribute() : base() { }
    }
}
