using System;

namespace BreadTh.StronglyApied.Direct.Core
{
    public class ModelAttributeException : Exception
    {
        public ModelAttributeException(string message) : base(message) { }
        public ModelAttributeException(string message, Exception inner) : base(message, inner) { }

    }
}
