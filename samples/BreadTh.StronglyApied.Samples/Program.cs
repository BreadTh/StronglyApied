﻿using System;
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
            Console.WriteLine(@"Press 'x' to run the XML example, any other key to run the JSON example");

            Action example = Console.ReadKey().KeyChar switch 
            {   'x'  => () => Example<MyXmlModel>(xmlInput)
            ,   _    => () => Example<MyJsonModel>(jsonInput)
            };

            example();
            Console.ReadKey();
        }

        static void Example<T>(string text)
        {
            IEnumerable<ValidationError> errors = new ModelValidator().TryParse(text, out T result);

            if (errors.Count() != 0)
                Console.WriteLine($"Invalid! :c\n\n{JsonConvert.SerializeObject(errors, Formatting.Indented)}");
            else
                Console.WriteLine("Valid! :D");

            Console.WriteLine($"\n\nParsed model: {JsonConvert.SerializeObject(result, Formatting.Indented)}");
        }

        const string xmlInput =
            @"<root>
                <stringChildren>
                    <good1>hello</good1>
                    <good2>10</good2>
                    <good3></good3>
                    <bad1><field>10</field></bad1>
                    <bad2><field>10</field><field>20</field></bad2>
                    <goodOptional1></goodOptional1>
                </stringChildren>
                <integerChildren>
                    <good1>10</good1>
                    <bad1>abc</bad1>
                    <bad2><field>10</field></bad2>
                    <bad3><field>10</field><field>20</field></bad3>
                    <bad4></bad4>
                    <goodOptional1></goodOptional1>
                </integerChildren>
                <decimalChildren>
                    <good1>12.34</good1>
                    <good2>10.00</good2>
                    <good3>10</good3>
                    <bad1>10</bad1>
                    <bad2>abc</bad2>
                    <bad3><field>10.10</field></bad3>
                    <bad4><field>10.00</field><field>20.00</field></bad4>
                    <bad5></bad5>
                    <goodOptional1></goodOptional1>
                </decimalChildren>
                <optionChildren>
                    <good1>Option1</good1>
                    <good2>Option2</good2>
                    <bad1>Option4</bad1>
                    <bad2>option1</bad2>
                    <bad3></bad3>
                    <bad4>Undefined</bad4>
                    <goodOptional1></goodOptional1>
                </optionChildren>
                <objectChildren>
                    <good1><field>value</field></good1>
                    <good2><field1>value</field1><field2>value</field2></good2>
                    <bad1>value</bad1>
                    <bad2><field>value1</field><field>value2</field></bad2>
                </objectChildren>
                <listChildren>
                    <good>1</good>
                    <good>2</good>
                    <good>3</good>
                    <bad1>1</bad1>
                    <bad1>2</bad1>
                    <bad1>value</bad1>
                    <bad2>1</bad2>
                    <bad2>2</bad2>
                    <bad2><field>3</field></bad2>
                    <bad3>1</bad3>
                    <bad3>2</bad3>
                    <bad3><field>3</field><field>3</field></bad3>
                    <bad4>1</bad4>
                    <bad4>2</bad4>
                    <bad4></bad4>
                    <goodOptional1>1</goodOptional1>
                    <goodOptional1>2</goodOptional1>
                    <goodOptional1></goodOptional1>
                </listChildren>
            </root>";

        public class MyXmlModel 
        {
            [StronglyApiedObject()] public StringChildren stringChildren;
            [StronglyApiedObject()] public IntegerChildren integerChildren;
            [StronglyApiedObject()] public DecimalChildren decimalChildren;
            [StronglyApiedObject()] public OptionChildren optionChildren;
            [StronglyApiedObject()] public ObjectChildren objectChildren;
            [StronglyApiedObject()] public ListChildren listChildren;

            public class StringChildren 
            {
                [StronglyApiedString()]               public string good1;
                [StronglyApiedString()]               public string good2;
                [StronglyApiedString()]               public string bad1;
                [StronglyApiedString()]               public string bad2;
                [StronglyApiedString()]               public string bad3;
                [StronglyApiedString(optional: true)] public string goodOptional1;
                [StronglyApiedString(optional: true)] public string goodOptional2;
            }

            public class IntegerChildren 
            {
                [StronglyApiedLong()]               public long good1;
                [StronglyApiedLong()]               public long bad1;
                [StronglyApiedLong()]               public long bad2;
                [StronglyApiedLong()]               public long bad3;
                [StronglyApiedLong()]               public long bad4;
                [StronglyApiedLong()]               public long bad5;
                [StronglyApiedLong(optional: true)] public long? goodOptional1;
                [StronglyApiedLong(optional: true)] public long? goodOptional2;
            }

            public class DecimalChildren 
            {
                [StronglyApiedDecimal(minDecimalDigits: 1)] public decimal good1;
                [StronglyApiedDecimal(minDecimalDigits: 1)] public decimal good2;
                [StronglyApiedDecimal()]                    public decimal good3;
                [StronglyApiedDecimal(minDecimalDigits: 1)] public decimal bad1;
                [StronglyApiedDecimal()]                    public decimal bad2;
                [StronglyApiedDecimal()]                    public decimal bad3;
                [StronglyApiedDecimal()]                    public decimal bad4;
                [StronglyApiedDecimal()]                    public decimal bad5;
                [StronglyApiedDecimal()]                    public decimal bad6;
                [StronglyApiedDecimal(optional: true)]      public decimal goodOptional1;
                [StronglyApiedDecimal(optional: true)]      public decimal goodOptional2;
            }

            public class OptionChildren 
            {
                public enum OptionValues { Undefined, Option1, Option2 }

                [StronglyApiedOption()]               public OptionValues good1;
                [StronglyApiedOption()]               public OptionValues good2;
                [StronglyApiedOption()]               public OptionValues bad1;
                [StronglyApiedOption()]               public OptionValues bad2;
                [StronglyApiedOption()]               public OptionValues bad3;
                [StronglyApiedOption()]               public OptionValues bad4;
                [StronglyApiedOption()]               public OptionValues bad5;
                [StronglyApiedOption(optional: true)] public OptionValues goodOptional1;
                [StronglyApiedOption(optional: true)] public OptionValues goodOptional2;
            }

            public class ObjectChildren 
            {
                [StronglyApiedObject()]               public OneField good1;
                [StronglyApiedObject()]               public TwoField good2;
                [StronglyApiedObject()]               public OneField bad1;
                [StronglyApiedObject()]               public OneField bad2;
                [StronglyApiedObject()]               public OneField bad3;
                [StronglyApiedObject(optional: true)] public OneField goodOptional;

                public class OneField
                {
                    [StronglyApiedString()] public string field;
                }

                public class TwoField 
                {
                    [StronglyApiedString()] public string field1;
                    [StronglyApiedString()] public string field2;
                } 
            }

            public class ListChildren 
            {
                [StronglyApiedArray(), StronglyApiedLong()]               public long[] good;
                [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad1;
                [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad2;
                [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad3;
                [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad4;
                [StronglyApiedArray(), StronglyApiedLong(optional: true)] public long?[] goodOptional1;
                [StronglyApiedArray(optional: true), StronglyApiedLong()] public long[] goodOptional2;
            }
        }

        const string jsonInput = 
            @"{
                ""strings"": {
                    ""good1"": ""hello"",
                    ""good2"": 10,
                    ""good3"": """",
                    ""bad1"": {""field"": ""value""},
                    ""bad2"": [""value""],
                    ""bad3"": null,
                    ""bad4"": undefined,
                    ""goodOptional"": null
                },
                ""integers"": {
                    ""good1"": 10,
                    ""good2"": ""10"",
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
                    ""bad2"" : ""abc"",
                    ""bad3"": {""field"": 10.00},
                    ""bad4"": [10.00],
                    ""bad5"" : null,
                    ""bad6"" : undefined,
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
                    ""good1"": {""field"": ""value""},
                    ""good2"": { },
                    ""bad1"": [""value""],
                    ""bad2"": ""value"",
                    ""bad3"": 10,
                    ""bad4"": null,
                    ""bad5"": undefined,
                    ""goodOptional"": null,
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
                [StronglyApiedString()]               public string good1;
                [StronglyApiedString()]               public string good2;
                [StronglyApiedString()]               public string good3;
                [StronglyApiedString()]               public string bad1;
                [StronglyApiedString()]               public string bad2;
                [StronglyApiedString()]               public string bad3;
                [StronglyApiedString()]               public string bad4;
                [StronglyApiedString()]               public string bad5;
                [StronglyApiedString(optional: true)] public string goodOptional;
            }

            public class Integers 
            {
                [StronglyApiedLong()]               public long good1;
                [StronglyApiedLong()]               public long good2;
                [StronglyApiedLong()]               public long bad1;
                [StronglyApiedLong()]               public long bad2;
                [StronglyApiedLong()]               public long bad3;
                [StronglyApiedLong()]               public long bad4;
                [StronglyApiedLong()]               public long bad5;
                [StronglyApiedLong()]               public long bad6;
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
                [StronglyApiedDecimal()]                    public decimal bad6;
                [StronglyApiedDecimal()]                    public decimal bad7;
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
                [StronglyApiedOption()]               public Value bad6;
                [StronglyApiedOption(optional: true)] public Value? goodOptional;
            }

            public class Objects 
            {
                [StronglyApiedObject()]               public InnerObject good1;
                [StronglyApiedObject()]               public EmptyObject good2;
                [StronglyApiedObject()]               public InnerObject bad1;
                [StronglyApiedObject()]               public InnerObject bad2;
                [StronglyApiedObject()]               public InnerObject bad3;
                [StronglyApiedObject()]               public InnerObject bad4;
                [StronglyApiedObject()]               public InnerObject bad5;
                [StronglyApiedObject()]               public InnerObject bad6;
                [StronglyApiedObject(optional: true)] public InnerObject goodOptional;

                public class InnerObject
                {
                    [StronglyApiedString()] public string field;
                }

                public class EmptyObject { }
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
                [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad8;
                [StronglyApiedArray(), StronglyApiedLong(optional: true)] public long?[] goodOptional1;
                [StronglyApiedArray(optional: true), StronglyApiedLong()] public long[] goodOptional2;
                [StronglyApiedArray(optional: true), StronglyApiedLong()] public long[] goodOptional3;
            }
        }
    }
}
