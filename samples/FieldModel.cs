using BreadTh.StronglyApied.Attributes;

namespace BreadTh.StronglyApied.Samples
{
    public class FieldModel
    {
        public Strings strings;
        public Integers integers;
        public Decimals decimals;
        public Options options;
        public Objects objects;
        public Arrays arrays;

        public class Strings
        {
            public string good1;
            public string good2;
            public string good3;
            public string bad1;           //incorrectly accepted
            public string bad2;           //incorrectly accepted
            public string bad3;
            public string bad4;
            public string bad5;
            public string? goodOptional;
        }

        public class Integers
        {
            public long good1;
            public long good2;
            public long bad1;
            public long bad2;
            public long bad3;
            public long bad4;
            public long bad5;
            public long bad6;
            public long? goodOptional;
        }

        public class Decimals
        {
            [StronglyApiedDecimal(minDecimalDigits: 1)]
            public decimal good1;

            [StronglyApiedDecimal(minDecimalDigits: 1)]
            public decimal good2; //Does not work with 10.00 as it will be parsed and rounded shortened to 10. Can be worked around by putting "10.00"

            public decimal good3;

            [StronglyApiedDecimal(minDecimalDigits: 1)]
            public decimal bad1;

            public decimal bad2;
            public decimal bad3;
            public decimal bad4;
            public decimal bad5;
            public decimal bad6;
            public decimal bad7;
            public decimal? goodOptional;
        }

        public class Options
        {
            public enum Value { Undefined, Option1, Option2, Option3 }

            public Value good1;
            public Value good2;
            public Value bad1;
            public Value bad2;
            public Value bad3;
            public Value bad4;
            public Value bad5;
            public Value bad6;
            public Value? goodOptional;
        }

        public class Objects
        {
            public InnerObject good1;
            public EmptyObject good2;
            public InnerObject bad1;
            public InnerObject bad2;
            public InnerObject bad3;
            public InnerObject bad4;
            public InnerObject bad5;
            public InnerObject bad6;
            public InnerObject? goodOptional;

            public class InnerObject
            {
                public string inner;
            }

            public class EmptyObject { }
        }

        public class Arrays
        {
            public long[] good;
            public long[] bad1;
            public long[] bad2;
            public long[] bad3;
            public long[] bad4;
            public long[] bad5;
            public long[] bad6;
            public long[] bad7;
            public long[] bad8;
            public long?[] goodOptional1;
            public long[]? goodOptional2;
            public long[]? goodOptional3;
        }
    }
}
