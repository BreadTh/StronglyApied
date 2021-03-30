using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BreadTh.StronglyApied
{
    public interface IModelValidator        
    {
        (T result, List<ErrorDescription> errors) Parse<T>(string rawbody);
        (object result, List<ErrorDescription> errors) Parse(string rawbody, Type type);
        Task<(T result, List<ErrorDescription> errors)> Parse<T>(Stream rawbody, bool leaveStreamOpen = true);
        Task<OUTCOME> Parse<OUTCOME, MODEL>(
            Stream rawbody
        ,   Func<List<ErrorDescription>, OUTCOME> onValidationError
        ,   Func<MODEL, Task<OUTCOME>> onSuccess
        ,   bool leaveStreamOpen = true);
        Task<OUTCOME> Parse<OUTCOME, MODEL>(
            string rawbody
        ,   Func<List<ErrorDescription>, OUTCOME> onValidationError
        ,   Func<MODEL, Task<OUTCOME>> onSuccess);
        List<ErrorDescription> ValidateModel<T>(T value);
    }
}
