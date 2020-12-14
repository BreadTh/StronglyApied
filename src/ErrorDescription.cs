using System;
using System.Globalization;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace BreadTh.StronglyApied
{
    public readonly struct ErrorDescription
    {
        public readonly string id;
        public readonly string description;
        public readonly string data;

        public ErrorDescription(string id, string description, string data)
        {
            this.id = id;
            this.description = description;
            this.data = data;
        }

        public ErrorDescription(string id, string description, object data) : this(id, description, JsonConvert.SerializeObject(data)){ }

        public static ErrorDescription Identity() =>
            new ErrorDescription("", "", "");
        public static ErrorDescription InvalidInputData(string data) =>
            new ErrorDescription("34877d2e-0014-4f6a-a9d7-1b9bdf63a502", "The given body data string is not valid JSON or XML", new { data });

        public static ErrorDescription OptionalViolation(string path) =>
            new ErrorDescription("5f161b44-9f43-40ad-b325-4463e514030b", $"The value at \"{path}\" may not be omitted or set to null/undefined", new { path });

        public static ErrorDescription StringTooShort(int minLength, string value, string path) =>
            new ErrorDescription("82b065b9-0a51-4182-a8f1-6ca7b23b0595"
                ,   $"The string-value (\"{value}\") at \"{path}\" is shorter than the expected minimum length of {minLength}. (after trimming)"
                ,   new { path, minLength, value });

        public static ErrorDescription StringTooLong(int maxLength, string value, string path) =>
            new ErrorDescription("32cbc5dd-3b5a-45e4-84a9-95e1dae6c725"
                ,   $"The string-value (\"{value}\") at \"{path}\" is longer than the expected maximum length of {maxLength}. (after trimming)"
                ,   new { path, maxLength, value });

        public static ErrorDescription InvalidInt64(string value, string path) =>
            new ErrorDescription("147f8d35-7f5a-4a4d-b170-ff806c818058"
                ,   $"Could not read the value (\"{value}\") at \"{path}\" as a signed 64-bit integer"
                ,   new { path, value });
        
        public static ErrorDescription InvalidInt32(string value, string path) =>
            new ErrorDescription("6eb475cf-8611-4244-80a1-0680d6bafe8a"
                ,   $"Could not read the value (\"{value}\") at \"{path}\" as a signed 32-bit integer"
                ,   new { path, value });

        public static ErrorDescription InvalidEmailAddress(string value, string path) =>
            new ErrorDescription("6eb475cf-8611-4244-80a1-0680d6bafe8a"
                ,   $"Could not read the value (\"{value}\") at \"{path}\" as an email address."
                ,   new { path, value });

        public static ErrorDescription NumericTooSmall(string actualValue, string minValue, string path) =>
            new ErrorDescription("f9568fe4-1f95-4d1c-8c97-b781efdcee7b" 
                ,   $"The numeric ({actualValue}) at \"{path}\" is smaller than the expected minimum value of {minValue}."
                ,   new { path, minValue, actualValue});
        public static ErrorDescription NumericTooSmall(long actualValue, long minValue, string path) =>
            NumericTooSmall(actualValue.ToString(), minValue.ToString(), path);
        public static ErrorDescription NumericTooSmall(decimal actualValue, decimal minValue, string path) =>
            NumericTooSmall(actualValue.ToString(CultureInfo.InvariantCulture), minValue.ToString(CultureInfo.InvariantCulture), path);
        public static ErrorDescription NumericTooLarge(string actualValue, string maxValue, string path) =>
            new ErrorDescription("37f61117-e128-4bb4-b22a-ad3021a39ec6" 
                ,   $"The numeric ({actualValue}) at \"{path}\" is larger than the expected maximum value of {maxValue}."
                ,   new { path, maxValue, actualValue});
        public static ErrorDescription NumericTooLarge(long actualValue, long maxValue, string path) =>
            NumericTooLarge(actualValue.ToString(), maxValue.ToString(), path);
        public static ErrorDescription NumericTooLarge(decimal actualValue, decimal maxValue, string path) =>
            NumericTooLarge(actualValue.ToString(CultureInfo.InvariantCulture), maxValue.ToString(CultureInfo.InvariantCulture), path);
        
        public static ErrorDescription InvalidBoolean(string value, string path) =>
            new ErrorDescription("b5e80fbe-c7ac-4685-957e-4fada53520f8"
                ,   $"The value at \"{path}\" must be boolean: false, true, 0 or 1"
                ,   new { path, value, options = new List<string>(){ "true", "false", "0", "1" }});

        public static ErrorDescription NotPrimitive(string path, string value) =>
             new ErrorDescription("0919dfa0-4212-4bc5-af44-269434f63cf7"
                ,   $"A primitive value (integer, decimal, string, boolean) was expected at \"{path}\", but instead got a data structure, \"{value}\"."
                ,   new { path, value });

        public static ErrorDescription NotAnArray(string value, string path) =>
            new ErrorDescription("fd91c75d-775d-4eaa-8c1c-c7bc14e18f3e"
                ,   $"An array was expected at \"{path}\" but another value-type was provided. (\"{value}\")"
                ,   new { path, value });

        public static ErrorDescription NotAnObject(string value, string path) =>
            new ErrorDescription("b248e9d3-143b-4de9-9bfe-6529d29aaed3"
                ,   $"An object was expected at \"{path}\" but another value-type was provided. (\"{value}\")"
                ,   new { path, value });
        public static ErrorDescription ArrayTooShort(long actualCount, long minCount, string path) =>
            new ErrorDescription("18895779-40f7-4e4c-9df6-509c7b33ac53" 
                ,   $"The array at \"{path}\" has fewer elements ({actualCount}) than the allowed {minCount}"
                ,   new { path, actualCount, minCount });
        public static ErrorDescription ArrayTooLong(long actualCount, long maxCount, string path) =>
            new ErrorDescription("b29d6fa9-8367-4b60-bfa9-8a6ea98ebe33" 
                ,   $"The array at \"{path}\" has more elements ({actualCount}) than the allowed {maxCount}"
                ,   new { path, actualCount, maxCount});

        public static ErrorDescription TooFewDecimalDigits(decimal value, int minDecimalDigits, string path) =>
            new ErrorDescription("16b1fa59-fd95-4981-9f0b-7ebd716f23a2"
                ,   $"The numeric ({value.ToString(CultureInfo.InvariantCulture)}) at \"{path}\" has fewer than the expected minimum decimal digits of {minDecimalDigits.ToString(CultureInfo.InvariantCulture)}."
                    +   "You may need to quote values ending on a zero to ensure they are transmitted correctly. ie {\"property\":\"0.50\"} instead of {\"property\": 0.50}."
                ,   new { path, value, minDecimalDigits });
        
        public static ErrorDescription TooManyDecimalDigits(decimal value, int maxDecimalDigits, string path) =>
            new ErrorDescription("13567846-43f3-4c66-8b75-1d3eaba24db4"
                ,   $"The numeric ({value.ToString(CultureInfo.InvariantCulture)}) at \"{path}\" has more than the expected maximum decimal digits of {maxDecimalDigits.ToString(CultureInfo.InvariantCulture)}"
                ,   new { path, value, maxDecimalDigits });

        public static ErrorDescription InvalidOption(string value, List<string> options, string path) =>
            new ErrorDescription("6863dffd-3a5e-40b2-b540-25575148ed7b"
                ,   $"The given value (\"{value}\") at \"{path}\" is not in the list of valid options."
                ,   new { path, value, options });

        public static ErrorDescription InternalError() => 
            new ErrorDescription("208d874d-8fde-4d7d-b1fe-1699c16a4da9"
                ,   "An internal error occoured in the logic layer. A system administrator has been notified and will investigate. Expect that the action you tried to perform didn't go through."
                ,   "{}");

        public static ErrorDescription Forbidden() =>
            new ErrorDescription("0bb1bc80-2d7a-421e-8108-c2fd4754f8f0"
                ,   "You do not have permission to perform the action you requested."
                ,   "{}");

        public static ErrorDescription RouteParameterRecordNotFound(string routeParameterName, string value) =>
            new ErrorDescription("504614b5-c624-4513-accd-42b325317e33"
                ,   "The corresponding record for the given route parameter value was not found. (or perhaps you do not have permission to it)"
                ,   JsonConvert.SerializeObject(new { routeParameterName, value }));

        public static ErrorDescription InvalidAppKey(string appkey) =>
            new ErrorDescription(
                "6a18afae-3371-47e2-829c-b28e4412f279"
            ,   "Invalid appkey or appsecret."
            ,   JsonConvert.SerializeObject(new { appkey }));

        public static ErrorDescription InvalidToken(string token) =>
            new ErrorDescription(
                "6164b9ab-8fb0-4949-99d6-ee5861eaeaba"
            ,   "The token has expired, been invalidated or never existed."
            ,   JsonConvert.SerializeObject(new { token }));

        public static ErrorDescription EmptyRouteParameter(string routeParameterName) =>
            new ErrorDescription(
                "f0a05407-b0e5-4411-8260-dfc86a74b159"
            ,   "The value following the given route parameter name may not be empty"
            ,   JsonConvert.SerializeObject(new { routeParameterName }));

        public static ErrorDescription MissingNonoptionalproperty(string propertyName) =>
            new ErrorDescription(
                "54809bfd-a5b1-4528-b9cb-fc29540118dd"
            ,   "A mandatory json-body property is missing."
            ,   JsonConvert.SerializeObject(new { propertyName }));

        public static ErrorDescription MissingNonoptionalHeader(string headerName) =>
            new ErrorDescription(
                "8c55dbb6-461c-4db7-bbe4-389247d5c80a"
            ,   "A mandatory header is missing."
            ,   JsonConvert.SerializeObject(new { headerName }));
        
        public static ErrorDescription MissingNonoptionalQueryValue(string queryName) =>
            new ErrorDescription(
                "90650b61-bb5c-486c-a0fe-b331fa1b844a"
            ,   "A mandatory query parameter is missing."
            ,   JsonConvert.SerializeObject(new { queryName }));

        public static ErrorDescription InvalidEmailaddressValue(string propertyName, string value) =>
            new ErrorDescription(
                "153df197-9cc1-4f1c-b408-0658bd58ea61"
            ,   "The value in the given property must be a valid email address."
            ,   JsonConvert.SerializeObject(new { propertyName, value }));

        public static ErrorDescription InvalidNonemptyEmailaddressValue(string propertyName, string value) =>
            new ErrorDescription(
                "38646897-dfbe-4446-b6d6-cb3a55a2b0aa"
            ,   "The value in the given property must be a valid email address or an empty string."
            ,   JsonConvert.SerializeObject(new { propertyName, value }));

        public static ErrorDescription TooManyElements(string propertyName, int maxValidCount, string value) =>
            new ErrorDescription(
                "d033e027-e8b0-4103-95a7-83abf6c77824"
            ,   "The given property contains too many elements."
            ,   JsonConvert.SerializeObject(new { propertyName, maxValidCount, value }));

        public static ErrorDescription TooFewElements(string propertyName, int minValidCount, string value) =>
            new ErrorDescription(
                "12be6d52-9dcb-496f-a046-15b24699e0e0"
            ,   "The given property contains too few elements."
            ,   JsonConvert.SerializeObject(new { propertyName, minValidCount, value }));

        public static ErrorDescription EmptyPropertyValue(string propertyName) =>
            new ErrorDescription(
                "ccd88481-f4e6-4fd5-9cbc-c0075112bf7b"
            ,   "The given json-body propery may not be empty string."
            ,   JsonConvert.SerializeObject(new { propertyName }));

        public static ErrorDescription RecordAlreadyExists(string fieldName, string value) =>
            new ErrorDescription(
                "bd0ad663-0455-4ced-9695-9d7042122342"
            ,   "A record with the given value for the given field name already exists."
            ,   JsonConvert.SerializeObject(new { fieldName, value }));

        public static ErrorDescription InvalidOption(string propertyName, string value, object[] validOptions) =>
            new ErrorDescription(
                "1ba5d29a-9509-448b-84c9-80b64f7c5de3"
            ,   "The given json-body property is not in list of valid options."
            ,   JsonConvert.SerializeObject(new { propertyName, value, validOptions }));

        public static ErrorDescription InvalidOption(string propertyName, string value, List<string> validOptions) =>
            InvalidOption(propertyName, value, validOptions.ToArray());

        public static ErrorDescription Nonnumeric(string propertyName, string value) =>
            new ErrorDescription(
                "aa933c62-2912-438d-a0f5-3aec815cc4cb"
            ,   "The value in the given property must be a signed int32 (natural number between -2147483648 and 2147483647)"
            ,   JsonConvert.SerializeObject(new { propertyName, value }));

        public static ErrorDescription NegativeNumeric(string propertyName, string value) =>
            new ErrorDescription(
                "97ad5536-db86-4b06-8c1e-4d146073a86e"
            ,   "The value in the given property must be >= 0"
            ,   JsonConvert.SerializeObject(new { propertyName, value }));

        public static ErrorDescription NumericPropertyValueTooSmall(string propertyName, string actualValue, string minValue) =>
            new ErrorDescription(
                "16a89ce5-61db-4432-bc1f-df1bab64d48b"
            ,   "The given property value is too small"
            ,   JsonConvert.SerializeObject(new { propertyName, actualValue, minValue }));

        public static ErrorDescription NumericPropertyValueTooLarge(string propertyName, string actualValue, string maxValue) =>
            new ErrorDescription(
                "3b720105-e9e9-4f6a-937f-e9b994bb6ca3"
            ,   "The given property value is too large"
            ,   JsonConvert.SerializeObject(new { propertyName, actualValue, maxValue }));

        public static ErrorDescription NumericQueryValueTooSmall(string queryName, string actualValue, string minValue) =>
            new ErrorDescription(
                "ff13d23f-607f-4fd1-84fc-5ba06aeb7174"
            ,   "The given query value is too small"
            ,   JsonConvert.SerializeObject(new { queryName, actualValue, minValue }));

        public static ErrorDescription NumericQueryValueTooLarge(string queryName, string actualValue, string maxValue) =>
            new ErrorDescription(
                "33fd8920-187e-4f67-95d3-5ace4c60febb"
            ,   "The given query value is too large"
            ,   JsonConvert.SerializeObject(new { queryName, actualValue, maxValue }));
    }
}
