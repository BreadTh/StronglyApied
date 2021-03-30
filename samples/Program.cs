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
            Console.WriteLine(@"Press 'x' to run the XML example or 'j' to run the JSON example");

            Action chosenExample = Console.ReadKey().KeyChar switch 
            {   'x'  => () => Direct<ExampleXmlModel>(ExampleXmlModel.exampleInput)
            ,   'j'  => () => Direct<ExampleJsonModel>(ExampleJsonModel.jsonInput)
            ,   _    => () => Console.WriteLine("SI didn't understand that key :(")
            };

            chosenExample();
            Console.ReadKey();
        }

        static void Direct<T>(string text)
        {
            (T result, IEnumerable<ErrorDescription> errors) = new ModelValidator().Parse<T>(text);

            if (errors.Count() != 0)
                Console.WriteLine($"Invalid! :c\n\n{JsonConvert.SerializeObject(errors, Formatting.Indented)}");
            else
                Console.WriteLine("Valid! :D");

            Console.WriteLine($"\n\nParsed model: {JsonConvert.SerializeObject(result, Formatting.Indented)}");
        }
    }
}
