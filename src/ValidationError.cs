using System;
using System.Globalization;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace BreadTh.StronglyApied
{
    public readonly struct ValidationError
    {
        public readonly string id;
        public readonly string description;
        public readonly string data;

        public ValidationError(string id, string description, string data)
        {
            this.id = id;
            this.description = description;
            this.data = data;
        }
        public ValidationError(string id, string description, object data) : this(id, description, JsonConvert.SerializeObject(data)){ }

        public static ValidationError Identity() =>
            new ValidationError("", "", "");
        public static ValidationError InvalidInputData(string data) =>
            new ValidationError("34877d2e-0014-4f6a-a9d7-1b9bdf63a502", "The given body data string is not valid JSON or XML", new { data });

        public static ValidationError OptionalViolation(string path) =>
            new ValidationError("5f161b44-9f43-40ad-b325-4463e514030b", $"The value at \"{path}\" may not be omitted or set to null/undefined", new { path });

        public static ValidationError StringTooShort(int minLength, string value, string path) =>
            new ValidationError("82b065b9-0a51-4182-a8f1-6ca7b23b0595"
                ,   $"The string-value (\"{value}\") at \"{path}\" is shorter than the expected minimum length of {minLength}. (after trimming)"
                ,   new { path, minLength, value });

        public static ValidationError StringTooLong(int maxLength, string value, string path) =>
            new ValidationError("32cbc5dd-3b5a-45e4-84a9-95e1dae6c725"
                ,   $"The string-value (\"{value}\") at \"{path}\" is longer than the expected maximum length of {maxLength}. (after trimming)"
                ,   new { path, maxLength, value });

        public static ValidationError InvalidInt64(string value, string path) =>
            new ValidationError("147f8d35-7f5a-4a4d-b170-ff806c818058"
                ,   $"Could not read the value (\"{value}\") at \"{path}\" as a signed 64-bit integer"
                ,   new { path, value });
        
        public static ValidationError NumericTooSmall(string actualValue, string minValue, string path) =>
            new ValidationError("f9568fe4-1f95-4d1c-8c97-b781efdcee7b" 
                ,   $"The numeric ({actualValue}) at \"{path}\" is smaller than the expected minimum value of {minValue}."
                ,   new { path, minValue, actualValue});
        public static ValidationError NumericTooSmall(long actualValue, long minValue, string path) =>
            NumericTooSmall(actualValue.ToString(), minValue.ToString(), path);
        public static ValidationError NumericTooSmall(decimal actualValue, decimal minValue, string path) =>
            NumericTooSmall(actualValue.ToString(CultureInfo.InvariantCulture), minValue.ToString(CultureInfo.InvariantCulture), path);
        public static ValidationError NumericTooLarge(string actualValue, string maxValue, string path) =>
            new ValidationError("37f61117-e128-4bb4-b22a-ad3021a39ec6" 
                ,   $"The numeric ({actualValue}) at \"{path}\" is larger than the expected maximum value of {maxValue}."
                ,   new { path, maxValue, actualValue});
        public static ValidationError NumericTooLarge(long actualValue, long maxValue, string path) =>
            NumericTooLarge(actualValue.ToString(), maxValue.ToString(), path);
        public static ValidationError NumericTooLarge(decimal actualValue, decimal maxValue, string path) =>
            NumericTooLarge(actualValue.ToString(CultureInfo.InvariantCulture), maxValue.ToString(CultureInfo.InvariantCulture), path);
        
        public static ValidationError InvalidBoolean(string value, string path) =>
            new ValidationError("b5e80fbe-c7ac-4685-957e-4fada53520f8"
                ,   $"The value at \"{path}\" must be boolean: false, true, 0 or 1"
                ,   new { path, value, options = new List<string>(){ "true", "false", "0", "1" }});

        public static ValidationError NotPrimitive(string path, string value) =>
             new ValidationError("0919dfa0-4212-4bc5-af44-269434f63cf7"
                ,   $"A primitive value (integer, decimal, string, boolean) was expected at \"{path}\", but instead got a data structure, \"{value}\"."
                ,   new { path, value });

        public static ValidationError NotAnArray(string value, string path) =>
            new ValidationError("fd91c75d-775d-4eaa-8c1c-c7bc14e18f3e"
                ,   $"An array was expected at \"{path}\" but another value-type was provided. (\"{value}\")"
                ,   new { path, value });

        public static ValidationError NotAnObject(string value, string path) =>
            new ValidationError("b248e9d3-143b-4de9-9bfe-6529d29aaed3"
                ,   $"An object was expected at \"{path}\" but another value-type was provided. (\"{value}\")"
                ,   new { path, value });
        public static ValidationError ArrayTooShort(long actualCount, long minCount, string path) =>
            new ValidationError("18895779-40f7-4e4c-9df6-509c7b33ac53" 
                ,   $"The array at \"{path}\" has fewer elements ({actualCount}) than the allowed {minCount}"
                ,   new { path, actualCount, minCount });
        public static ValidationError ArrayTooLong(long actualCount, long maxCount, string path) =>
            new ValidationError("b29d6fa9-8367-4b60-bfa9-8a6ea98ebe33" 
                ,   $"The array at \"{path}\" has more elements ({actualCount}) than the allowed {maxCount}"
                ,   new { path, actualCount, maxCount});

        public static ValidationError TooFewDecimalDigits(decimal value, int minDecimalDigits, string path) =>
            new ValidationError("16b1fa59-fd95-4981-9f0b-7ebd716f23a2"
                ,   $"The numeric ({value.ToString(CultureInfo.InvariantCulture)}) at \"{path}\" has fewer than the expected minimum decimal digits of {minDecimalDigits.ToString(CultureInfo.InvariantCulture)}."
                    +   "You may need to quote values ending on a zero to ensure they are transmitted correctly. ie {\"property\":\"0.50\"} instead of {\"property\": 0.50}."
                ,   new { path, value, minDecimalDigits });
        
        public static ValidationError TooManyDecimalDigits(decimal value, int maxDecimalDigits, string path) =>
            new ValidationError("13567846-43f3-4c66-8b75-1d3eaba24db4"
                ,   $"The numeric ({value.ToString(CultureInfo.InvariantCulture)}) at \"{path}\" has more than the expected maximum decimal digits of {maxDecimalDigits.ToString(CultureInfo.InvariantCulture)}"
                ,   new { path, value, maxDecimalDigits });

        public static ValidationError InvalidOption(string value, List<string> options, string path) =>
            new ValidationError("6863dffd-3a5e-40b2-b540-25575148ed7b"
                ,   $"The given value (\"{value}\") at \"{path}\" is not in the list of valid options."
                ,   new { path, value, options });

        public static ValidationError InternalError() => 
            new ValidationError("208d874d-8fde-4d7d-b1fe-1699c16a4da9"
                ,   "An internal error occoured in the logic layer. A system administrator has been notified and will investigate. Expect that the action you tried to perform didn't go through."
                ,   "{}");

        public static ValidationError Forbidden() =>
            new ValidationError("0bb1bc80-2d7a-421e-8108-c2fd4754f8f0"
                ,   "You do not have permission to perform the action you requested."
                ,   "{}");

        public static ValidationError RouteParameterRecordNotFound(string routeParameterName, string value) =>
            new ValidationError("504614b5-c624-4513-accd-42b325317e33"
                ,   "The corresponding record for the given route parameter value was not found. (or perhaps you do not have permission to it)"
                ,   JsonConvert.SerializeObject(new { routeParameterName, value }));

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(ValidationError left, ValidationError right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ValidationError left, ValidationError right)
        {
            return !(left == right);
        }
    }
}
