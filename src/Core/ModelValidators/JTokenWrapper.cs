using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

using Newtonsoft.Json.Linq;

using BreadTh.StronglyApied.Attributes.Extending;

namespace BreadTh.StronglyApied.Core.ModelValidators
{
    public class JTokenWrapper : IToken
    {
        private JToken _token;
        public JTokenWrapper(JToken token)
        {
            _token = token;
        }

        public bool IsNullOrUndefinedAsPrimitive() => 
            _token == null || _token.Type == JTokenType.Null || _token.Type == JTokenType.Undefined;
               
        public bool IsNullOrUndefinedAsObject() => 
            IsNullOrUndefinedAsPrimitive();

        public bool IsChildAsArrayNullOrUndefined(string childName)
        {
            JToken child = _token.SelectToken(childName);
            return child == null || child.Type == JTokenType.Null || child.Type == JTokenType.Undefined;
        }
        
        public bool IsChildArray(string childName)
        {
            JToken child = _token.SelectToken(childName);
            return child != null && child.Type == JTokenType.Array;
        }

        public bool IsPrimitive() =>
            _token.Type != JTokenType.Array && _token.Type != JTokenType.Object;

        public bool IsObject() =>
            _token.Type == JTokenType.Object;

        public IToken GetChild(string name) =>
            new JTokenWrapper(_token.SelectToken(name));
        public IEnumerable<IToken> GetChildren(string name)
        {
            JToken childContainer = ((JObject)_token).SelectToken(name);
            return ((JArray)childContainer).Children().Select(child => new JTokenWrapper(child));
        }

        public IToken GetAttribute(string name) =>
            throw new InvalidOperationException("JSON models can't have attributes, only children/properties");

        public override string ToString() =>
            _token.GetType() == typeof(JValue) 
            ?   ((JValue)_token).ToString(CultureInfo.InvariantCulture)
            :   _token.ToString();

        public bool IsChildPrimitive(string childName) =>
            new JTokenWrapper(_token.SelectToken(childName)).IsPrimitive();

        public bool IsChildNullOrUndefined(string childName) =>
            new JTokenWrapper(_token.SelectToken(childName)).IsNullOrUndefinedAsPrimitive();

        public string GetChildAsText(string childName) =>
            _token.SelectToken(childName)?.ToString() ?? "null";
    }
}