using FsCheck;
using System.Linq;

namespace BreadTh.StronglyApied.Tests.Tools
{
    public static class CSharpFieldNameGenerator
    {
        public static Arbitrary<string> Generate() =>
            Arb.Default.String().Filter((string str) =>
                str != null
            &&  str.Length != 0
            &&  (   (str[0] >= 'A' && str[0] <= 'Z')
                ||  (str[0] >= 'a' && str[0] <= 'z')
                ||  str[0] == '_'
                )
            &&  str.Skip(1).All(c => 
                    (c >= 'A' && c <= 'Z')
                ||  (c >= 'a' && c <= 'z')
                ||  (c >= '0' && c <= '9')
                ||  c == '_'
                )
            );
    }
}