using System;

namespace BreadTh.StronglyApied.Attributes.Core
{
    public abstract class StronglyApiedRelationBaseAttribute : Attribute
    {
        public readonly string name;
        public StronglyApiedRelationBaseAttribute(string name)
        {
            this.name = name;
        }
    }
}
