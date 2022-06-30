using BreadTh.StronglyApied.Attributes;

namespace BreadTh.StronglyApied.Samples
{
    public class PropertyModel
    {
        public Strings strings { get; set; }
        public Integers integers { get; set; }
        public Decimals decimals { get; set; }
        public Options options { get; set; }
        public Objects objects { get; set; }
        public Arrays arrays { get; set; }

        public class Strings
        {
            public string good1 { get; set; }
            public string good2 { get; set; }
            public string good3 { get; set; }
            public string bad1 { get; set; }           //incorrectly accepted
            public string bad2 { get; set; }           //incorrectly accepted
            public string bad3 { get; set; }
            public string bad4 { get; set; }
            public string bad5 { get; set; }
            public string? goodOptional { get; set; }
        }

        public class Integers
        {
            public long good1 { get; set; }
            public long good2 { get; set; }
            public long bad1 { get; set; }
            public long bad2 { get; set; }
            public long bad3 { get; set; }
            public long bad4 { get; set; }
            public long bad5 { get; set; }
            public long bad6 { get; set; }
            public long? goodOptional { get; set; }
        }

        public class Decimals
        {
            [StronglyApiedDecimal(minDecimalDigits: 1)]
            public decimal good1 { get; set; }

            [StronglyApiedDecimal(minDecimalDigits: 1)]
            public decimal good2 { get; set; } //Does not work with 10.00 as it will be parsed and rounded shortened to 10. Can be worked around by putting "10.00"

            public decimal good3 { get; set; }

            [StronglyApiedDecimal(minDecimalDigits: 1)]
            public decimal bad1 { get; set; }

            public decimal bad2 { get; set; }
            public decimal bad3 { get; set; }
            public decimal bad4 { get; set; }
            public decimal bad5 { get; set; }
            public decimal bad6 { get; set; }
            public decimal bad7 { get; set; }
            public decimal? goodOptional { get; set; }
        }

        public class Options
        {
            public enum Value { Undefined, Option1, Option2, Option3 }

            public Value good1 { get; set; }
            public Value good2 { get; set; }
            public Value bad1 { get; set; }
            public Value bad2 { get; set; }
            public Value bad3 { get; set; }
            public Value bad4 { get; set; }
            public Value bad5 { get; set; }
            public Value bad6 { get; set; }

            public Value? goodOptional { get; set; }
        }

        public class Objects
        {
            public InnerObject good1 { get; set; }
            public EmptyObject good2 { get; set; }
            public InnerObject bad1 { get; set; }
            public InnerObject bad2 { get; set; }
            public InnerObject bad3 { get; set; }
            public InnerObject bad4 { get; set; }
            public InnerObject bad5 { get; set; }
            public InnerObject bad6 { get; set; }
            public InnerObject? goodOptional { get; set; }

            public class InnerObject
            {
                public string inner { get; set; }
            }

            public class EmptyObject { }
        }

        public class Arrays
        {
            public long[] good { get; set; }
            public long[] bad1 { get; set; }
            public long[] bad2 { get; set; }
            public long[] bad3 { get; set; }
            public long[] bad4 { get; set; }
            public long[] bad5 { get; set; }
            public long[] bad6 { get; set; }
            public long[] bad7 { get; set; }
            public long[] bad8 { get; set; }
            public long?[] goodOptional1 { get; set; }
            public long[]? goodOptional2 { get; set; }
            public long[]? goodOptional3 { get; set; }
        }
    }
}
