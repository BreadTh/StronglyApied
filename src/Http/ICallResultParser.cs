using System;
using System.Collections.Generic;
using System.Net;

using BreadTh.StronglyApied.Http.Core;

namespace BreadTh.StronglyApied.Http
{
    public interface ICallResultParser<OUTCOME>
    {
        CallResultParser<OUTCOME> HandleHttpStatus(Func<HttpStatusCode, bool> shouldHttpStatusCodeBeHandled, Func<string, OUTCOME> transform);
        CallResultParser<OUTCOME> HandleHttpStatus(Func<HttpStatusCode, bool> shouldHttpStatusCodeBeHandled, Func<SuccessfulHttpCallContext, OUTCOME> transform);
        OUTCOME OnNoMatch(Func<string, OUTCOME> transform);
        OUTCOME OnNoMatch(Func<SuccessfulHttpCallContext, List<KeyValuePair<string, List<ValidationError>>>, OUTCOME> transform);
        CallResultParser<OUTCOME> TryMatchResponseBodyWithModel<MODEL>(Func<MODEL, OUTCOME> transformOnSuccessfulModelParse);
    }
}