using System;
using System.Net;

using RestSharp;

namespace BreadTh.StronglyApied.Http.Core
{
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
}
