using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace BreadTh.StronglyApied.Databases.Redis
{
    public struct TrySetEntryResult
    {
        public enum Status { Undefined, Ok, ValidationError }
            
        public static TrySetEntryResult Ok() =>
            new TrySetEntryResult(Status.Ok, new List<ValidationError>());

        public static TrySetEntryResult ValidationError(List<ValidationError> validationErrors) =>
            new TrySetEntryResult(Status.ValidationError, validationErrors);
            
        [JsonConverter(typeof(StringEnumConverter))]
        public Status status;
        public List<ValidationError> validationErrors;

        public TrySetEntryResult(Status status, List<ValidationError> validationErrors)
        {
            this.status = status;
            this.validationErrors = validationErrors;
        }
    }
}
