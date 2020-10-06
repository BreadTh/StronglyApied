using System;
using RestSharp;

using BreadTh.StronglyApied.Http.Core;
using BreadTh.StronglyApied.Direct;

namespace BreadTh.StronglyApied.Http
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
}
