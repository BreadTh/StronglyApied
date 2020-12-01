using System;
using System.Net;
using System.Collections.Generic;

using Newtonsoft.Json;

using BreadTh.StronglyApied.Direct;

namespace BreadTh.StronglyApied.Http.Core
{
    public class CallResultParser<OUTCOME> : ICallResultParser<OUTCOME>
    {
        IModelValidator _modelValidator;

        readonly SuccessfulHttpCallContext _context;
        readonly List<KeyValuePair<string, List<ValidationError>>> _validationErrorsOverModelNames = new List<KeyValuePair<string, List<ValidationError>>>();
        OutcomeCarrier<OUTCOME> _outcomeCarrier;

        public CallResultParser(OutcomeCarrier<OUTCOME> outcomeCarrier, SuccessfulHttpCallContext context, IModelValidator modelValidator = null)
        {
            _outcomeCarrier = outcomeCarrier;
            _context = context;
            _modelValidator = modelValidator ?? new ModelValidator();
        }

        public CallResultParser<OUTCOME> HandleHttpStatus(Func<HttpStatusCode, bool> shouldHttpStatusCodeBeHandled, Func<SuccessfulHttpCallContext, OUTCOME> transform)
        {
            if (_outcomeCarrier.status != OutcomeCarrier<OUTCOME>.Status.AlreadyFound)
                if (shouldHttpStatusCodeBeHandled(_context.statusCode))
                    _outcomeCarrier = OutcomeCarrier<OUTCOME>.AlreadyFound(transform(_context));

            return this;
        }

        public CallResultParser<OUTCOME> HandleHttpStatus(Func<HttpStatusCode, bool> shouldHttpStatusCodeBeHandled, Func<string, OUTCOME> transform) =>
            HandleHttpStatus(shouldHttpStatusCodeBeHandled, (SuccessfulHttpCallContext context) => transform("Unexpected HTTP status encounted during call: " + JsonConvert.SerializeObject(context)));

        public CallResultParser<OUTCOME> TryMatchResponseBodyWithModel<MODEL>(Func<MODEL, OUTCOME> transformOnSuccessfulModelParse)
        {
            if (_outcomeCarrier.status != OutcomeCarrier<OUTCOME>.Status.AlreadyFound)
            {
                (MODEL model, List<ValidationError> validationErrors) = _modelValidator.TryParse<MODEL>(_context.responseBody);

                if (validationErrors.Count == 0)
                    _outcomeCarrier = OutcomeCarrier<OUTCOME>.AlreadyFound(transformOnSuccessfulModelParse(model));
                else
                    _validationErrorsOverModelNames.Add(new KeyValuePair<string, List<ValidationError>>(typeof(MODEL).FullName, validationErrors));
            }
            return this;
        }

        public OUTCOME OnNoMatch(Func<SuccessfulHttpCallContext, List<KeyValuePair<string, List<ValidationError>>>, OUTCOME> transform)
        {
            if (_outcomeCarrier.status == OutcomeCarrier<OUTCOME>.Status.AlreadyFound)
                return _outcomeCarrier.outcome;
            return transform(_context, _validationErrorsOverModelNames);
        }

        public OUTCOME OnNoMatch(Func<string, OUTCOME> transform) =>
            OnNoMatch((SuccessfulHttpCallContext context, List<KeyValuePair<string, List<ValidationError>>> _validationErrorsOverModelNames) =>
                transform($"No matching model was found when parsing http call: {JsonConvert.SerializeObject(context)}. The following validation/matching issues were found: {JsonConvert.SerializeObject(_validationErrorsOverModelNames)}"));
    }
}
