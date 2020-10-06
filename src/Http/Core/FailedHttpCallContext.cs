using System;

using RestSharp;

namespace BreadTh.StronglyApied.Http.Core
{
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
}
