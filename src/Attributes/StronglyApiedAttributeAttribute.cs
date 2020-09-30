using System;

using BreadTh.StronglyApied.Attributes.Core;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StronglyApiedAttributeAttribute : StronglyApiedRelationBaseAttribute
    {
        public StronglyApiedAttributeAttribute(string name) : base(name) { }
    }
}
