using System;
using System.Net;

namespace BreadTh.StronglyApied.Http.Core
{
    public class CallResultParserWithStringlyErrorHandling<OUTCOME> : ICallResultParserWithStringlyErrorHandling<OUTCOME>
    {
        private ICallResultParser<OUTCOME> _actualParser;
        private Func<string, OUTCOME> _stringlyErrorHandler;

        public CallResultParserWithStringlyErrorHandling(ICallResultParser<OUTCOME> actualParser, Func<string, OUTCOME> stringlyErrorHandler)
        {
            _actualParser = actualParser;
            _stringlyErrorHandler = stringlyErrorHandler;
        }

        public CallResultParserWithStringlyErrorHandling<OUTCOME> HandleHttpStatus(Func<HttpStatusCode, bool> shouldHttpStatusCodeBeHandled, Func<string, OUTCOME> transform = null)
        {
            _actualParser.HandleHttpStatus(shouldHttpStatusCodeBeHandled, transform ?? _stringlyErrorHandler);
            return this;
        }

        public CallResultParserWithStringlyErrorHandling<OUTCOME> TryMatchResponseBodyWithModel<MODEL>(Func<MODEL, OUTCOME> transformOnSuccessfulModelParse)
        {
            _actualParser.TryMatchResponseBodyWithModel(transformOnSuccessfulModelParse);
            return this;
        }

        public OUTCOME GetResult() =>
            _actualParser.OnNoMatch(_stringlyErrorHandler);

    }
}
