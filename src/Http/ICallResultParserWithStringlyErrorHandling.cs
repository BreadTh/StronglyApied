using System;
using System.Net;

using BreadTh.StronglyApied.Http.Core;

namespace BreadTh.StronglyApied.Http
{
    public interface ICallResultParserWithStringlyErrorHandling<OUTCOME>
    {
        OUTCOME GetResult();
        CallResultParserWithStringlyErrorHandling<OUTCOME> HandleHttpStatus(Func<HttpStatusCode, bool> shouldHttpStatusCodeBeHandled, Func<string, OUTCOME> transform = null);
        CallResultParserWithStringlyErrorHandling<OUTCOME> TryMatchResponseBodyWithModel<MODEL>(Func<MODEL, OUTCOME> transformOnSuccessfulModelParse);
    }
}