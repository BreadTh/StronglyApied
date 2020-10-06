using System.Collections.Generic;

namespace BreadTh.StronglyApied.Databases.Redis
{
    public enum TryGetStatus { Undefined, Ok, NotFound, NotValidJson, ValidationError }
    public readonly struct GetEntryResult<T>
    {
        public static GetEntryResult<T> Ok(T result) =>
            new GetEntryResult<T>(TryGetStatus.Ok, result, new List<ValidationError>());

        internal static GetEntryResult<T> NotFound() =>
            new GetEntryResult<T>(TryGetStatus.NotFound, default, new List<ValidationError>());

        public static GetEntryResult<T> NotValidJson() =>
            new GetEntryResult<T>(TryGetStatus.NotValidJson, default, new List<ValidationError>());

        public static GetEntryResult<T> ValidationError(List<ValidationError> errorList, T result) =>
            new GetEntryResult<T>(TryGetStatus.ValidationError, result, errorList);

        public readonly TryGetStatus status;
        public readonly T result;
        public readonly List<ValidationError> validationErrors;

        public GetEntryResult(TryGetStatus status, T result, List<ValidationError> validationErrors)
        {
            this.status = status;
            this.result = result;
            this.validationErrors = validationErrors;
        }

        public static bool operator ==(GetEntryResult<T> lhs, GetEntryResult<T> rhs) => lhs.Equals(rhs);
        public static bool operator !=(GetEntryResult<T> lhs, GetEntryResult<T> rhs) => !lhs.Equals(rhs);
        public override bool Equals(object obj) => obj is GetEntryResult<T> && Equals((GetEntryResult<T>) obj);
        private bool Equals(GetEntryResult<T> other) => other.AsTuple().Equals(AsTuple());
        public override int GetHashCode() => AsTuple().GetHashCode();

        private (TryGetStatus, T) AsTuple() =>
            (status, result);
    }
}
