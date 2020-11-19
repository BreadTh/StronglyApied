using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace BreadTh.StronglyApied.Direct
{
    public interface IModelValidator
    {
        List<ValidationError> TryParse<T>(Stream rawbody, out T result);
        List<ValidationError> TryParse<T>(string rawbody, out T result);
        Task<OUTCOME> TryParse<OUTCOME, MODEL>(Stream rawbody, Func<List<ValidationError>, OUTCOME> onValidationError, Func<MODEL, Task<OUTCOME>> onSuccess);
        Task<OUTCOME> TryParse<OUTCOME, MODEL>(string rawbody, Func<List<ValidationError>, OUTCOME> onValidationError, Func<MODEL, Task<OUTCOME>> onSuccess);
        
        List<ValidationError> ValidateModel<T>(T value);
    }
}
