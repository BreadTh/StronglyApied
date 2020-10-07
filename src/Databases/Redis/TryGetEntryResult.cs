using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BreadTh.StronglyApied.Databases.Redis
{
    public enum TryGetStatus { Undefined, Ok, NotFound, NotValidJson, ValidationError }
    public readonly struct TryGetEntryResult<T>
    {
        public static TryGetEntryResult<T> Ok(T result) =>
            new TryGetEntryResult<T>(TryGetStatus.Ok, result, new List<ValidationError>());

        internal static TryGetEntryResult<T> NotFound() =>
            new TryGetEntryResult<T>(TryGetStatus.NotFound, default, new List<ValidationError>());

        public static TryGetEntryResult<T> NotValidJson() =>
            new TryGetEntryResult<T>(TryGetStatus.NotValidJson, default, new List<ValidationError>());

        public static TryGetEntryResult<T> ValidationError(List<ValidationError> errorList, T result) =>
            new TryGetEntryResult<T>(TryGetStatus.ValidationError, result, errorList);

        [JsonConverter(typeof(StringEnumConverter))]
        public readonly TryGetStatus status;
        public readonly T result;
        public readonly List<ValidationError> validationErrors;

        public TryGetEntryResult(TryGetStatus status, T result, List<ValidationError> validationErrors)
        {
            this.status = status;
            this.result = result;
            this.validationErrors = validationErrors;
        }
    }
}