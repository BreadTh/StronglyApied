using System;
using System.Collections.Generic;
using System.Text;

namespace BreadTh.StronglyApied.Attributes.Core
{
    public abstract class StronglyApiedRelationBaseAttribute : Attribute
    {
        protected readonly string name;
        public StronglyApiedRelationBaseAttribute(string name)
        {
            this.name = name;
        }
    }
}
