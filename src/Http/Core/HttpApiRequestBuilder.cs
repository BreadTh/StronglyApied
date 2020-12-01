using System;
using System.Threading;

using RestSharp;
using Newtonsoft.Json;

using BreadTh.StronglyApied.Direct;

namespace BreadTh.StronglyApied.Http.Core
{
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

        public IHttpApiRequestBuilder<OUTCOME> AddParameter(string name, string value)
        {
            _request.AddParameter(name, value);
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
}
