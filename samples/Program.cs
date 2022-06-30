using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace BreadTh.StronglyApied.Samples
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("==============================");
            Console.WriteLine("         PropertyModel        ");
            Console.WriteLine("==============================");
            ParsePrint<PropertyModel>(jsonInput);

            Console.WriteLine("==============================");
            Console.WriteLine("          FieldModel          ");
            Console.WriteLine("==============================");
            ParsePrint<FieldModel>(jsonInput);


            Console.ReadKey();
        }

        static void ParsePrint<T>(string text)
        {
            (T result, List<ErrorDescription> errors) = new ModelValidator().Parse<T>(text);

            if (errors.Count() != 0)
                Console.WriteLine($"Invalid:\n{JsonConvert.SerializeObject(errors, Formatting.Indented)}");
            else
                Console.WriteLine("Valid!");

            Console.WriteLine($"\n\nParsed model: {JsonConvert.SerializeObject(result, Formatting.Indented)}");
        }

        public const string jsonInput =
            @"{
                ""strings"": {
                    ""good1"": ""hello"",
                    ""good2"": 10,
                    ""good3"": """",
                    ""bad1"": {""inner"": ""value""},
                    ""bad2"": [""value""],
                    ""bad3"": null,
                    ""bad4"": undefined,
                    ""goodOptional"": null
                },
                ""integers"": {
                    ""good1"": 10,
                    ""good2"": ""10"",
                    ""bad1"": ""abc"",
                    ""bad2"": {""inner"": 10},
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
                    ""bad3"": {""inner"": 10.00},
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
                    ""good1"": {""inner"": ""value""},
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
                    ""bad2"": [1, 2, {""inner"": 3}],
                    ""bad3"": [1, 2, [3]],
                    ""bad4"": [1, 2, null],
                    ""bad5"": [1, 2, undefined],
                    ""bad6"": null,
                    ""bad7"": undefined,
                    ""goodOptional1"": [1, 2, null, undefined],
                    ""goodOptional2"": null
                }
            }";
    }
}
