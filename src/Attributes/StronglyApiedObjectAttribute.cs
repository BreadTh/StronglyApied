using BreadTh.StronglyApied.Attributes.Extending.Core;
using System;

namespace BreadTh.StronglyApied.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)] 
    public sealed class StronglyApiedObjectAttribute : StronglyApiedBaseAttribute
    {
        public readonly bool stringified;
        public StronglyApiedObjectAttribute(string name = null, bool optional = false, bool stringified = false)
            : base(name, optional)
        {
            this.stringified = stringified;
        }
    }
}
