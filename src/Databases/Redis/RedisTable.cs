using System;
using System.Linq;
using System.Threading.Tasks;

using StackExchange.Redis;
using Newtonsoft.Json;

using BreadTh.StronglyApied.Direct;

namespace BreadTh.StronglyApied.Databases.Redis
{
    public class RedisTable<ENTRY>
    {
        readonly IConnectionMultiplexer _connectionMultiplexer;
        readonly string _keyPrefix;
        readonly TimeSpan? _durability;

        public RedisTable(IConnectionMultiplexer multiplexer, string keyPrefix, TimeSpan? durability)
        {
            _connectionMultiplexer = multiplexer;
            _keyPrefix = keyPrefix;
            _durability = durability;
        }
       
        public GetEntryResult<ENTRY> Get(string key)
        {
            string resultRaw = _connectionMultiplexer.GetDatabase().StringGet(_keyPrefix + key);
            
            if (string.IsNullOrWhiteSpace(resultRaw))
                return GetEntryResult<ENTRY>.NotFound();
            
            var errorList = new ModelValidator().TryParse(resultRaw, out ENTRY result).ToList();

            if(errorList.Count == 0)
                return GetEntryResult<ENTRY>.Ok(result);

            if(errorList.Count == 1 && errorList[0].id == "34877d2e-0014-4f6a-a9d7-1b9bdf63a502")
                return GetEntryResult<ENTRY>.NotValidJson();

            return GetEntryResult<ENTRY>.ValidationError(errorList, result);
        }

        public async Task Set(string key, ENTRY value) =>
            await _connectionMultiplexer.GetDatabase().StringSetAsync(_keyPrefix + key, JsonConvert.SerializeObject(value), _durability);

        public async Task Delete(string key) =>
            await _connectionMultiplexer.GetDatabase().KeyDeleteAsync(_keyPrefix + key);

        public GetEntryResult<ENTRY> GetAndDelete(string key)
        {
            GetEntryResult<ENTRY> result = Get(key);

            if (result.status == TryGetStatus.Ok)
                _connectionMultiplexer.GetDatabase().KeyDelete(_keyPrefix + key);

            return result;
        }
    }
}
