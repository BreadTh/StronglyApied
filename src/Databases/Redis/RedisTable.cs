using System;
using System.Linq;
using System.Threading.Tasks;

using StackExchange.Redis;
using Newtonsoft.Json;

using BreadTh.StronglyApied.Direct;
using System.Collections.Generic;

namespace BreadTh.StronglyApied.Databases.Redis
{
    public class RedisTable<ENTRY>
    {
        private static IConnectionMultiplexer defaultConnectionMultiplexer;

        public static RedisTable<ENTRY> Connect(string keyPrefix, TimeSpan? durability, IConnectionMultiplexer connectionMultiplexer = null)
        {
            if(connectionMultiplexer != null)
                return new RedisTable<ENTRY>(connectionMultiplexer, keyPrefix, durability, new ModelValidator());

            if(defaultConnectionMultiplexer == null)
                defaultConnectionMultiplexer = ConnectionMultiplexer.Connect("localhost");

            return new RedisTable<ENTRY>(defaultConnectionMultiplexer, keyPrefix, durability, new ModelValidator());
        }

        readonly IConnectionMultiplexer _connectionMultiplexer;
        readonly string _keyPrefix;
        readonly TimeSpan? _durability;
        readonly IModelValidator _validator;

        private RedisTable(IConnectionMultiplexer multiplexer, string keyPrefix, TimeSpan? durability, IModelValidator validator)
        {
            _connectionMultiplexer = multiplexer;
            _keyPrefix = keyPrefix;
            _durability = durability;
            _validator = validator;
        }
       
        public async Task<TryGetEntryResult<ENTRY>> TryGet(string key)
        {
            string resultRaw = await _connectionMultiplexer.GetDatabase().StringGetAsync(_keyPrefix + key);
            
            if (string.IsNullOrWhiteSpace(resultRaw))
                return TryGetEntryResult<ENTRY>.NotFound();

            (ENTRY result, List<ValidationError> errorList) = _validator.TryParse<ENTRY>(resultRaw);

            if(errorList.Count == 0)
                return TryGetEntryResult<ENTRY>.Ok(result);

            if(errorList.Count == 1 && errorList[0].id == "34877d2e-0014-4f6a-a9d7-1b9bdf63a502")
                return TryGetEntryResult<ENTRY>.NotValidJson();

            return TryGetEntryResult<ENTRY>.ValidationError(errorList, result);
        }

        public async Task<TrySetEntryResult> TrySet(string key, ENTRY value, bool forceSetValueEvenWhenValidationError = false) 
        {
            List<ValidationError> validationErrors = _validator.ValidateModel(value);

            if(validationErrors.Count == 0)
            {
                await PerformSet();
                return TrySetEntryResult.Ok();
            }
            else if(forceSetValueEvenWhenValidationError) 
                await PerformSet();
            
            return TrySetEntryResult.ValidationError(validationErrors);
            
            async Task PerformSet() =>
                await _connectionMultiplexer.GetDatabase().StringSetAsync(_keyPrefix + key, JsonConvert.SerializeObject(value), _durability);
        }

        public async Task Delete(string key) =>
            await _connectionMultiplexer.GetDatabase().KeyDeleteAsync(_keyPrefix + key);

        public async Task<TryGetEntryResult<ENTRY>> TryGetAndDelete(string key)
        {
            TryGetEntryResult<ENTRY> result = await TryGet(key);

            if (result.status == TryGetStatus.Ok)
                await _connectionMultiplexer.GetDatabase().KeyDeleteAsync(_keyPrefix + key);

            return result;
        }
    }
}
