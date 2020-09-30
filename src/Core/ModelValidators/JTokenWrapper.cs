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

        public bool IsNullOrUndefined() => 
            _token == null || _token.Type == JTokenType.Null || _token.Type == JTokenType.Undefined;
        public bool IsArray() =>
            _token.Type == JTokenType.Array;

        public bool IsPrimitive() =>
            _token.Type != JTokenType.Array && _token.Type != JTokenType.Object;

        public bool IsObject() =>
            _token.Type == JTokenType.Object;

        public IToken GetChild(string name) =>
            new JTokenWrapper(_token.SelectToken(name));
        public IEnumerable<IToken> GetChildren() => 
            ((JArray)_token).Children().Select(child => new JTokenWrapper(child));

        public IToken GetAttribute(string name) =>
            throw new Exception("Json models cannot have attributes, only children (aka properties).");

        public override string ToString() =>
            _token.GetType() == typeof(JValue) 
            ?   ((JValue)_token).ToString(CultureInfo.InvariantCulture)
            :   _token.ToString();
    }
}