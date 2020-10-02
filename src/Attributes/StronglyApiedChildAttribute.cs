using BreadTh.StronglyApied.Attributes.Core;
using System;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedChildAttribute : StronglyApiedRelationBaseAttribute
    {
        public StronglyApiedChildAttribute(string name = null) : base(name) { }
    }
}
