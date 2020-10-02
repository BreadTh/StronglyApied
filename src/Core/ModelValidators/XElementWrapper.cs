﻿using System;
using System.Collections.Generic;

using BreadTh.StronglyApied.Attributes.Extending;
using System.Xml.Linq;
using System.Text;
using System.Linq;

namespace BreadTh.StronglyApied.Core.ModelValidators
{
    public class XElementWrapper : IToken
    {
        private XElement _token;
        public XElementWrapper(XElement token)
        {
            _token = token;
        }

        public bool IsNullOrUndefinedAsPrimitive() => 
            _token == null || _token.FirstNode == null;

        public bool IsNullOrUndefinedAsObject() => 
            _token == null;

        public bool IsChildArray(string childName) =>
            !IsPrimitive(); //since an empty list is always possible then.
        
        public bool IsChildAsArrayNullOrUndefined(string childName) => 
            IsPrimitive(); //.. as above.
        
        public bool IsObject() =>
            !IsPrimitive(); //.. and again.

        public bool IsPrimitive()
        {           
            if(_token.FirstNode == null)
                return true;

            if(_token.FirstNode.NextNode != null)
                return false;

            return _token.FirstNode.NodeType == System.Xml.XmlNodeType.Text;
        }

        public bool IsChildPrimitive(string childName)
        {
            List<XElement> children = _token.Elements(XName.Get(childName)).ToList();

            if(children.Count == 0)
                return true;

            if(children.Count > 1)
                return false;

            return new XElementWrapper(children[0]).IsPrimitive();
        }

        public bool IsChildNullOrUndefined(string childName) => 
            _token.Element(XName.Get(childName)) == null;
        

        public IToken GetChild(string name) =>
            new XElementWrapper(_token.Element(XName.Get(name)));

        public IEnumerable<IToken> GetChildren(string name) => 
            _token.Elements(XName.Get(name)).Select(token => new XElementWrapper(token));

        public IToken GetAttribute(string name) =>
            throw new NotImplementedException();

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            for(XNode next = _token.FirstNode; next != null; next = next.NextNode)
                result.Append(next.ToString());
            return result.ToString();
        }

        public string GetChildAsText(string childName) =>
            string.Concat(_token.Elements(XName.Get(childName)).Select(child => child.ToString()));
    }
}