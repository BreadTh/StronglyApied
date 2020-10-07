using System.IO;
using System.Collections.Generic;

namespace BreadTh.StronglyApied.Direct
{
    public interface IModelValidator
    {
        List<ValidationError> TryParse<T>(Stream xmlOrJson, out T result);
        List<ValidationError> TryParse<T>(string xmlOrJson, out T result);
        List<ValidationError> ValidateModel<T>(T value);
    }
}
