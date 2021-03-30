using System;

namespace BreadTh.StronglyApied.Attributes
{
    public enum DataModel {Json, Xml}

    [AttributeUsage(AttributeTargets.Class)] 
    public sealed class StronglyApiedRootAttribute : Attribute
    {
        public readonly DataModel datamodel;
        public StronglyApiedRootAttribute(DataModel datamodel)
        {
            this.datamodel = datamodel;
        }
    }
}
