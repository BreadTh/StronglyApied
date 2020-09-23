using System;

using RestSharp;

namespace BreadTh.StronglyApied
{
    public interface IHttpApiClient
    {
        IHttpApiClient SetBaseUrl(string baseUrl);
        IHttpApiClient AddDefaultHeader(string name, string value);
        IHttpApiRequestBuilder<OUTCOME> CreateRequest<OUTCOME>(string webPath, Method httpMethod);
        IHttpApiRequestBuilderWithStringlyErrorHandling<OUTCOME> CreateRequest<OUTCOME>(string webPath, Method httpMethod, Func<string, OUTCOME> stringlyErrorHandler);
    }
}
