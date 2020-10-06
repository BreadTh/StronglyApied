using BreadTh.StronglyApied.Direct.Attributes.Core;
using System;

namespace BreadTh.StronglyApied.Direct.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedChildAttribute : StronglyApiedRelationBaseAttribute
    {
        public StronglyApiedChildAttribute(string name = null) : base(name) { }
    }
}
