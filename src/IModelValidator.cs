using System.IO;
using System.Collections.Generic;

namespace BreadTh.StronglyApied
{
    public interface IModelValidator
    {
        IEnumerable<ValidationError> TryParse<T>(Stream json, out T result);
        IEnumerable<ValidationError> TryParse<T>(string json, out T result);
    }
}
