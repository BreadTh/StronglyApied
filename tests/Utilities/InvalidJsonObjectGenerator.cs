using FsCheck;

using Newtonsoft.Json.Linq;

namespace BreadTh.StronglyApied.Tests.Tools
{
    public static class InvalidJsonObjectGenerator
    {
        public static Arbitrary<string> Generate() =>
            Arb.Default.String().Filter((string str) =>
                {
                    try
                    {
                        _ = JObject.Parse(str);
                        return false;
                    }
                    catch 
                    {
                        return true;
                    }
                });
    }
}