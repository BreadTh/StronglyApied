using System;

using BreadTh.StronglyApied.Attributes.Core;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedXmlElementAttribute : StronglyApiedXmlRelationBaseAttribute
    {
        public StronglyApiedXmlElementAttribute() : base() { }
    }
}
