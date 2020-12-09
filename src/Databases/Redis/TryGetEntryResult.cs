using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BreadTh.StronglyApied.Databases.Redis
{
    public enum TryGetStatus { Undefined, Ok, NotFound, NotValidJson, ValidationError }
    public readonly struct TryGetEntryResult<T>
    {
        public static TryGetEntryResult<T> Ok(T result) =>
            new TryGetEntryResult<T>(TryGetStatus.Ok, result, new List<ErrorDescription>());

        internal static TryGetEntryResult<T> NotFound() =>
            new TryGetEntryResult<T>(TryGetStatus.NotFound, default, new List<ErrorDescription>());

        public static TryGetEntryResult<T> NotValidJson() =>
            new TryGetEntryResult<T>(TryGetStatus.NotValidJson, default, new List<ErrorDescription>());

        public static TryGetEntryResult<T> ValidationError(List<ErrorDescription> errorList, T result) =>
            new TryGetEntryResult<T>(TryGetStatus.ValidationError, result, errorList);

        [JsonConverter(typeof(StringEnumConverter))]
        public readonly TryGetStatus status;
        public readonly T result;
        public readonly List<ErrorDescription> validationErrors;

        public TryGetEntryResult(TryGetStatus status, T result, List<ErrorDescription> validationErrors)
        {
            this.status = status;
            this.result = result;
            this.validationErrors = validationErrors;
        }
    }
}