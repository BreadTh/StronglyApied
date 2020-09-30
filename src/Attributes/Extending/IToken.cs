using System.Collections.Generic;

namespace BreadTh.StronglyApied.Attributes.Extending
{
    public interface IToken
    {
        bool IsNullOrUndefined();
        bool IsArray();
        bool IsPrimitive();
        bool IsObject();
        IToken GetChild(string name);
        IEnumerable<IToken> GetChildren();
        IToken GetAttribute(string name);
        string ToString();
    }
}
