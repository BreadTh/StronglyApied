using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

using BreadTh.StronglyApied;
using BreadTh.StronglyApied.Attributes;

namespace Samples
{
    class Program
    {
        static void Main()
        {
            string input = "{\"array\":[1,10,\"oh\"]}";

            IEnumerable<ValidationError> errors = new ModelValidator().TryParse(input, out MyModel result);

            if (errors.Count() == 0)
                Console.WriteLine($"Valid! :D\n\n{JsonConvert.SerializeObject(result, Formatting.Indented)}");
            else
                Console.WriteLine($"Invalid! :c\n\n{JsonConvert.SerializeObject(errors, Formatting.Indented)}");
        }

        public class MyModel
        {
            [StronglyApiedArray(maxLength: 2), StronglyApiedLong(minValue: 5)]
            public long[] @array;
        }
    }
}
