using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BreadTh.StronglyApied.Http.Core
{
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
