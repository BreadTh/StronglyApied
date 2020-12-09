using System;

namespace BreadTh.StronglyApied.Attributes.Extending
{
    public abstract class StronglyApiedFieldBase : Attribute
    {
        public bool optional;

        protected StronglyApiedFieldBase(bool optional)
        {
            this.optional = optional;
        }

        public abstract TryParseResult TryParse(Type type, string value, string path);

        public readonly struct TryParseResult
        {
            public static TryParseResult Ok(dynamic result) =>
                new TryParseResult(Status.Ok, result, ErrorDescription.Identity());

            public static TryParseResult Invalid(ErrorDescription error) =>
                new TryParseResult(Status.Invalid, null, error);

            public readonly Status status;
            public readonly dynamic result;
            public readonly ErrorDescription error;

            private TryParseResult(Status status, dynamic result, ErrorDescription error)
            {
                this.status = status;
                this.result = result;
                this.error = error;
            }
            public enum Status { Undefined, Ok, Invalid }
        }
    }
}
