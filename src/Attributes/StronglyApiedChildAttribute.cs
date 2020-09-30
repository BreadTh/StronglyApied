using System;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field)] 
    public sealed class StronglyApiedChildAttribute : Attribute
    {
        private readonly string name;
        public StronglyApiedChildAttribute(string name = null)
        {
            this.name = name;
        }
    }
}
