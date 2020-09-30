using System.IO;
using System.Collections.Generic;

namespace BreadTh.StronglyApied
{
    public interface IModelValidator
    {
        IEnumerable<ValidationError> TryParse<T>(Stream xmlOrJson, out T result);
        IEnumerable<ValidationError> TryParse<T>(string xmlOrJson, out T result);
    }
}
