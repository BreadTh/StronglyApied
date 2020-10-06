using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using BreadTh.StronglyApied.Direct;

namespace BreadTh.StronglyApied.Samples
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine(@"Press 'x' to run the XML example, 'r' to run the redis example, or 'j' to run the JSON example");

            Action example = Console.ReadKey().KeyChar switch 
            {   'x'  => () => Direct<ExampleXmlModel>(ExampleXmlModel.exampleInput)
            ,   'j'  => () => Direct<ExampleJsonModel>(ExampleJsonModel.jsonInput)
            ,   'r'  => () => RedisExample.Example()
            ,   _    => () => Console.WriteLine("I didn't understand that key :(")
            };

            example();
            Console.ReadKey();
        }

        static void Direct<T>(string text)
        {
            IEnumerable<ValidationError> errors = new ModelValidator().TryParse(text, out T result);

            if (errors.Count() != 0)
                Console.WriteLine($"Invalid! :c\n\n{JsonConvert.SerializeObject(errors, Formatting.Indented)}");
            else
                Console.WriteLine("Valid! :D");

            Console.WriteLine($"\n\nParsed model: {JsonConvert.SerializeObject(result, Formatting.Indented)}");
        }
    }
}
