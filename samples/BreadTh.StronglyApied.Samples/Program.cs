using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

using BreadTh.StronglyApied;
using BreadTh.StronglyApied.Attributes;

namespace Samples
{
    partial class Program
    {
        static void Main()
        {
            string jsonInput = 
                @"{
                    ""strings"": {
                        ""good"": ""hello"",
                        ""goodToo"": 10,
                        ""bad1"": {""field"": ""value""},
                        ""bad2"": [""value""],
                        ""bad3"": null,
                        ""bad4"": undefined,
                        ""goodOptional"": null
                    },
                    ""integers"": {
                        ""good"": 10,
                        ""goodToo"": ""10"",
                        ""bad1"": ""abc"",
                        ""bad2"": {""field"": 10},
                        ""bad3"": [10],
                        ""bad4"": null,
                        ""bad5"": undefined,
                        ""goodOptional"": null
                    },
                    ""decimals"": {
                        ""good1"": 12.34,
                        ""good2"": ""10.00"",
                        ""good3"": 10,
                        ""bad1"" : 10,
                        ""bad2"": {""field"": 10.00},
                        ""bad3"": [10.00],
                        ""bad4"" : null,
                        ""bad5"" : undefined,
                        ""goodOptional"" : null
                    },
                    ""options"": {
                        ""good1"": ""Option1"",
                        ""good2"": ""Option2"",
                        ""bad1"": ""Option4"",
                        ""bad2"": ""option1"",
                        ""bad3"": null,
                        ""bad4"": undefined,
                        ""bad5"": ""Undefined"",
                        ""goodOptional"": null
                    },
                    ""objects"": {
                        ""good"": {""field"": ""value""},
                        ""bad1"": [""value""],
                        ""bad2"": ""value"",
                        ""bad3"": 10,
                        ""bad4"": null,
                        ""bad5"": undefined,
                        ""goodOptional"": null,
                        ""goodDynamic1"": {""field"": ""value""},
                        ""goodDynamic2"": [""value""],
                    },
                    ""arrays"":{
                        ""good"": [1, 2, 3],
                        ""bad1"": [1, 2, ""value""],
                        ""bad2"": [1, 2, {""field"": 3}],
                        ""bad3"": [1, 2, [3]],
                        ""bad4"": [1, 2, null],
                        ""bad5"": [1, 2, undefined],
                        ""bad6"": null,
                        ""bad7"": undefined,
                        ""goodOptional1"": [1, 2, null, undefined],
                        ""goodOptional2"": null
                    }
                }";

            IEnumerable<ValidationError> errors = new ModelValidator().TryParse(jsonInput, out MyJsonModel result);

            if (errors.Count() != 0)
                Console.WriteLine($"Invalid! :c\n\n{JsonConvert.SerializeObject(errors, Formatting.Indented)}");
            else
                Console.WriteLine("Valid! :D");

            Console.WriteLine($"\n\nParsed model: {JsonConvert.SerializeObject(result, Formatting.Indented)}");
        }

        public class MyJsonModel
        {
            [StronglyApiedObject()] public Strings strings;
            [StronglyApiedObject()] public Integers integers;
            [StronglyApiedObject()] public Decimals decimals;
            [StronglyApiedObject()] public Options options;
            [StronglyApiedObject()] public Objects objects;
            [StronglyApiedObject()] public Arrays arrays;

            public class Strings
            {
                [StronglyApiedString()]               public string good;
                [StronglyApiedString()]               public string goodToo;
                [StronglyApiedString()]               public string bad1;
                [StronglyApiedString()]               public string bad2;
                [StronglyApiedString()]               public string bad3;
                [StronglyApiedString()]               public string bad4;
                [StronglyApiedString(optional: true)] public string goodOptional;
            }

            public class Integers 
            {
                [StronglyApiedLong()]               public long good;
                [StronglyApiedLong()]               public long goodToo;
                [StronglyApiedLong()]               public long bad1;
                [StronglyApiedLong()]               public long bad2;
                [StronglyApiedLong()]               public long bad3;
                [StronglyApiedLong()]               public long bad4;
                [StronglyApiedLong()]               public long bad5;
                [StronglyApiedLong(optional: true)] public long? goodOptional;
            }

            public class Decimals
            {
                [StronglyApiedDecimal(minDecimalDigits: 1)] public decimal good1;
                [StronglyApiedDecimal(minDecimalDigits: 1)] public decimal good2; //Does not work with 10.00 as it will be parsed and rounded shortened to 10. Can be worked around by putting "10.00"
                [StronglyApiedDecimal()]                    public decimal good3;
                [StronglyApiedDecimal(minDecimalDigits: 1)] public decimal bad1;
                [StronglyApiedDecimal()]                    public decimal bad2;
                [StronglyApiedDecimal()]                    public decimal bad3;
                [StronglyApiedDecimal()]                    public decimal bad4;
                [StronglyApiedDecimal()]                    public decimal bad5;
                [StronglyApiedDecimal(optional: true)]      public decimal? goodOptional;
            }

            public class Options
            {
                public enum Value { Undefined, Option1, Option2, Option3 }

                [StronglyApiedOption()]               public Value good1;
                [StronglyApiedOption()]               public Value good2;
                [StronglyApiedOption()]               public Value bad1;
                [StronglyApiedOption()]               public Value bad2;
                [StronglyApiedOption()]               public Value bad3;
                [StronglyApiedOption()]               public Value bad4;
                [StronglyApiedOption()]               public Value bad5;
                [StronglyApiedOption(optional: true)] public Value? goodOptional;
            }

            public class Objects 
            {
                [StronglyApiedObject()]               public InnerObject good;
                [StronglyApiedObject()]               public InnerObject bad1;
                [StronglyApiedObject()]               public InnerObject bad2;
                [StronglyApiedObject()]               public InnerObject bad3;
                [StronglyApiedObject()]               public InnerObject bad4;
                [StronglyApiedObject()]               public InnerObject bad5;
                [StronglyApiedObject(optional: true)] public InnerObject goodOptional;
                [StronglyApiedObject()]               public dynamic goodDynamic1;
                [StronglyApiedObject()]               public dynamic goodDynamic2;

                public class InnerObject
                {
                    [StronglyApiedString()] public string field;
                }
            }

            public class Arrays 
            {
                [StronglyApiedArray(), StronglyApiedLong()]               public long[] good;
                [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad1;
                [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad2;
                [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad3;
                [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad4;
                [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad5;
                [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad6;
                [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad7;
                [StronglyApiedArray(), StronglyApiedLong(optional: true)] public long?[] goodOptional1;
                [StronglyApiedArray(optional: true), StronglyApiedLong()] public long[] goodOptional2;
            }
        }
    }
}
