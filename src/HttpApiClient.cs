using System;
using System.Net;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BreadTh.StronglyApied
{
    public class HttpApiClient : IHttpApiClient
    {
        IRestClient _restClient;
        IModelValidator _modelValidator;

        public HttpApiClient(IRestClient restClient = null, IModelValidator validator = null)
        {
            _restClient = restClient ?? new RestClient();
            _modelValidator = validator ?? new ModelValidator();
        }

        public IHttpApiClient SetBaseUrl(string baseUrl)
        {
            _restClient.BaseUrl = new Uri(baseUrl);
            return this;
        }

        public IHttpApiClient AddDefaultHeader(string name, string value)
        {
            _restClient.AddDefaultHeader(name, value);
            return this;
        }

        public IHttpApiRequestBuilder<OUTCOME> CreateRequest<OUTCOME>(string webPath, Method httpMethod) =>
            new HttpApiRequestBuilder<OUTCOME>(_restClient, new RestRequest(webPath, httpMethod), _modelValidator);

        public IHttpApiRequestBuilderWithStringlyErrorHandling<OUTCOME> CreateRequest<OUTCOME>(string webPath, Method httpMethod, Func<string, OUTCOME> stringlyErrorHandler) =>
            new HttpApiRequestBuilderWithStringlyErrorHandling<OUTCOME>(new HttpApiRequestBuilder<OUTCOME>(_restClient, new RestRequest(webPath, httpMethod), _modelValidator), stringlyErrorHandler);
    }

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

        public ICallResultParserWithStringlyErrorHandling<OUTCOME> PerformCall() =>
            new CallResultParserWithStringlyErrorHandling<OUTCOME>(_actualBuilder.PerformCall(_stringlyErrorHandler), _stringlyErrorHandler);

    }

    public class HttpApiRequestBuilder<OUTCOME> : IHttpApiRequestBuilder<OUTCOME>
    {
        readonly IRestClient _client;
        readonly IRestRequest _request;
        readonly IModelValidator _modelValidator;
        public HttpApiRequestBuilder(IRestClient client, IRestRequest request, IModelValidator modelValidator = null)
        {
            _client = client;
            _request = request;
            _modelValidator = modelValidator ?? new ModelValidator();
        }

        public IHttpApiRequestBuilder<OUTCOME> AddJsonBody(object body)
        {
            _request.AddJsonBody(body);
            return this;
        }

        public IHttpApiRequestBuilder<OUTCOME> AddHeader(string name, string value)
        {
            _request.AddHeader(name, value);
            return this;
        }
        
        public ICallResultParser<OUTCOME> PerformCall(Func<FailedHttpCallContext, OUTCOME> onTransitError, TimeSpan[] retrySpacing = null)
        {
            retrySpacing ??= new []{ TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5)};

            int transmissionErrorRetries = 0;
            int connectionTimeoutErrorRetries = 0;

            IRestResponse response;

            while (true)
            {
                response = _client.Execute(_request);               

                if(response.ResponseStatus == ResponseStatus.Completed)
                    return new CallResultParser<OUTCOME>(OutcomeCarrier<OUTCOME>.NotYetFound(), new SuccessfulHttpCallContext(_client, _request, response), _modelValidator);

                else if(response.ResponseStatus == ResponseStatus.TimedOut)
                {
                    if(connectionTimeoutErrorRetries == retrySpacing.Length)
                        break;

                    Thread.Sleep(retrySpacing[connectionTimeoutErrorRetries]);
                    connectionTimeoutErrorRetries++;
                }
                else if(response.ResponseStatus == ResponseStatus.Error)
                {
                    if(transmissionErrorRetries == retrySpacing.Length)
                        break;

                    Thread.Sleep(retrySpacing[transmissionErrorRetries]);
                    transmissionErrorRetries++;
                }
                
                else
                    throw new NotImplementedException($"Unhandled ResponseStatus: {response.ResponseStatus}");    
            } 

            return new CallResultParser<OUTCOME>(
                OutcomeCarrier<OUTCOME>.AlreadyFound(
                    onTransitError(
                        new FailedHttpCallContext(_client, _request, response)))
                ,   default
                ,   _modelValidator);
        }

        public ICallResultParser<OUTCOME> PerformCall(Func<string, OUTCOME> onTransitError, TimeSpan[] retrySpacing = null) =>
            PerformCall((FailedHttpCallContext context) => onTransitError("Transit error during call: " + JsonConvert.SerializeObject(context)), retrySpacing);
    }

    public readonly struct OutcomeCarrier<OUTCOME>
    {
        public enum Status { Undefined, NotYetFound, AlreadyFound }

        public static OutcomeCarrier<OUTCOME> AlreadyFound(OUTCOME outcome) =>
            new OutcomeCarrier<OUTCOME>(Status.AlreadyFound, outcome);

        public static OutcomeCarrier<OUTCOME> NotYetFound() =>
            new OutcomeCarrier<OUTCOME>(Status.NotYetFound, default);


        public readonly Status status;
        public readonly OUTCOME outcome;

        private OutcomeCarrier(Status status, OUTCOME outcome)
        {
            this.status = status;
            this.outcome = outcome;
        }
    }

    public readonly struct FailedHttpCallContext
    {
        public readonly Method method;
        public readonly string path;
        public readonly string requestBody;
        public readonly ResponseStatus responseStatus;

        public FailedHttpCallContext(IRestClient client, IRestRequest request, IRestResponse response)
        {
            method = request.Method;
            path = client.BaseUrl + request.Resource;
            requestBody = request.Body?.ToString() ?? "";
            responseStatus = response.ResponseStatus;
        }
    }

    public readonly struct SuccessfulHttpCallContext
    {
        public readonly Method method;
        public readonly string path;
        public readonly string requestBody;
        public readonly string responseBody;
        public readonly HttpStatusCode statusCode;

        public SuccessfulHttpCallContext(IRestClient client, IRestRequest request, IRestResponse response)
        {
            method = request.Method;
            path = client.BaseUrl + request.Resource;
            requestBody = request.Body?.ToString() ?? "";
            responseBody = response.Content;
            statusCode = response.StatusCode;
        }
    }

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
                List<ValidationError> validationErrors = _modelValidator.TryParse(_context.responseBody, out MODEL model).ToList();

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

    public readonly struct HttpApiError<MODEL>
    {
        public enum HttpApiResponseStatus { Undefined, HttpTimeout, HttpTransitError, HttpStatusError, ModelValidationError, InternalError }
        
        public HttpApiError(HttpApiResponseStatus status, MODEL result, string errorMessage)
        {
            this.status = status;
            this.result = result;
            this.errorMessage = errorMessage;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public readonly HttpApiResponseStatus status;
        public readonly MODEL result;
        public readonly string errorMessage;
    }
}
