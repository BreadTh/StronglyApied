using BreadTh.StronglyApied.Attributes.Core;
using System;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedXmlElementAttribute : StronglyApiedXmlRelationBaseAttribute
    {
        public StronglyApiedXmlElementAttribute() : base() { }
    }
}
