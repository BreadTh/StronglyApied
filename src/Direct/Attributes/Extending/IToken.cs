using System.Collections.Generic;

namespace BreadTh.StronglyApied.Direct.Attributes.Extending
{
    public interface IToken
    {
        bool IsNullOrUndefinedAsPrimitive();
        bool IsNullOrUndefinedAsObject();
        bool IsChildAsArrayNullOrUndefined(string childName);
        bool IsChildArray(string childName);
        bool IsPrimitive();
        bool IsObject();
        bool IsChildPrimitive(string childName);
        bool IsChildNullOrUndefined(string childName);
        IToken GetChild(string name);
        string GetChildAsText(string childName);
        IEnumerable<IToken> GetChildren(string name);
        string GetAttribute(string name);
        string ToString();
        bool IsAttributeNullOrUndefined(string name);
    }
}
