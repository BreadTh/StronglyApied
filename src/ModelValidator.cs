using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using BreadTh.StronglyApied.Core;

namespace BreadTh.StronglyApied
{
    public class ModelValidator : IModelValidator
    {
        public async Task<OUTCOME> Parse<OUTCOME, MODEL>(
            Stream rawbody
        ,   Func<List<ErrorDescription>, OUTCOME> onValidationError
        ,   Func<MODEL, Task<OUTCOME>> onSuccess    
        ,   bool leaveStreamOpen = true
        ) =>
            await ModelValidatorImp.Parse<OUTCOME, MODEL>(rawbody, onValidationError, onSuccess, leaveStreamOpen);
        
        public async Task<OUTCOME> Parse<OUTCOME, MODEL>(
            string rawbody
        ,   Func<List<ErrorDescription>, OUTCOME> onValidationError
        ,   Func<MODEL, Task<OUTCOME>> onSuccess
        ) =>
            await ModelValidatorImp.Parse<OUTCOME, MODEL>(rawbody, onValidationError, onSuccess);

        public async Task<(MODEL result, List<ErrorDescription> errors)> Parse<MODEL>(Stream rawbody, bool leaveStreamOpen = true) =>
            await ModelValidatorImp.Parse<MODEL>(rawbody, leaveStreamOpen);

        public (MODEL result, List<ErrorDescription> errors) Parse<MODEL>(string rawbody) =>
            ModelValidatorImp.Parse<MODEL>(rawbody);

        public (object result, List<ErrorDescription> errors) Parse(string rawbody, Type type) =>
            ModelValidatorImp.Parse(rawbody, type);

        public List<ErrorDescription> ValidateModel<MODEL>(MODEL value) =>
            ModelValidatorImp.ValidateModel<MODEL>(value);

    }
}
