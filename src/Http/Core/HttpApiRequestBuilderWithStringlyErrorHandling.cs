using System;

namespace BreadTh.StronglyApied.Http.Core
{
    public class HttpApiRequestBuilderWithStringlyErrorHandling<OUTCOME> : IHttpApiRequestBuilderWithStringlyErrorHandling<OUTCOME>
    {
        readonly HttpApiRequestBuilder<OUTCOME> _actualBuilder;
        readonly Func<string, OUTCOME> _stringlyErrorHandler;

        public HttpApiRequestBuilderWithStringlyErrorHandling(HttpApiRequestBuilder<OUTCOME> actualBuilder, Func<string, OUTCOME> stringlyErrorHandler)
        {
            _actualBuilder = actualBuilder;
            _stringlyErrorHandler = stringlyErrorHandler;
        }

        public IHttpApiRequestBuilderWithStringlyErrorHandling<OUTCOME> AddJsonBody(object body)
        {
            _actualBuilder.AddJsonBody(body);
            return this;
        }

        public IHttpApiRequestBuilderWithStringlyErrorHandling<OUTCOME> AddHeader(string name, string value)
        {
            _actualBuilder.AddHeader(name, value);
            return this;
        }

        public IHttpApiRequestBuilderWithStringlyErrorHandling<OUTCOME> AddParameter(string name, string value)
        {
            _actualBuilder.AddParameter(name, value);
            return this;
        }


        public ICallResultParserWithStringlyErrorHandling<OUTCOME> PerformCall() =>
            new CallResultParserWithStringlyErrorHandling<OUTCOME>(_actualBuilder.PerformCall(_stringlyErrorHandler), _stringlyErrorHandler);

    }
}
