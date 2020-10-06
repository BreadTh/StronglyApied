using System;

namespace BreadTh.StronglyApied.Direct.Attributes
{
    public enum DataModel { Undefined, XML, JSON}

    [AttributeUsage(AttributeTargets.Class)] 
    public sealed class StronglyApiedRootAttribute : Attribute
    {
        public DataModel datamodel;
        public StronglyApiedRootAttribute(DataModel datamodel)
        {
            this.datamodel = datamodel;
        }
    }
}
