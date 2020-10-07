using System;
using System.Threading.Tasks;

using StackExchange.Redis;
using Newtonsoft.Json;

using BreadTh.StronglyApied.Databases.Redis;
using BreadTh.StronglyApied.Direct.Attributes;

namespace BreadTh.StronglyApied.Samples
{
    class RedisExample
    {
        public static async Task Example()
        {
            ConnectionMultiplexer.Connect("localhost").GetDatabase().StringSet("Users::Joan", "{\"password\": password123\",\"permissions\":\"Break it all!\"}");
            //Invalid json at here---------------------------------------------------------------------------^

            DataSources dataSources = new DataSources();
            
            Console.WriteLine("\n\n Writing records..");
            
            Console.WriteLine("\n\n" + JsonConvert.SerializeObject(
                await dataSources.Users.TrySet("John", new User() 
                {   permissions = "Do anything!!"
                ,   password = "NramkJcVCKVTcqYiWDOuRLOlHSMRPqx" 
                })
            ));

            Console.WriteLine("\n\n" + JsonConvert.SerializeObject(
                await dataSources.Users.TrySet("Steve", new User() 
                {   permissions = null
                ,   password = "   pass1!   " 
                })
            ));

            Console.Write("\n\nWrite the name of the user record you want to read: ");
            string recordName = Console.ReadLine();

            TryGetEntryResult<User> userLookup = await dataSources.Users.TryGet(recordName);
            
            Console.WriteLine(
                userLookup.status switch
                {   TryGetStatus.Ok                 => $"Record at \"{recordName}\" exists. They're allowed to \"{userLookup.result.permissions}\""
                ,   TryGetStatus.NotFound           => $"Record at \"{recordName}\" doesn't exist."
                ,   TryGetStatus.NotValidJson       => $"Record at \"{recordName}\" is not valid JSON."
                ,   TryGetStatus.ValidationError    => $"The record at \"{recordName}\" was valid JSON, but did not match the expected format. Here's why: \n\n"
                                                        +   JsonConvert.SerializeObject(userLookup.validationErrors, Formatting.Indented)
                                                        +   $"\nRegardless, we can still read the parts that did validate: {JsonConvert.SerializeObject(userLookup.result)}"
                ,   _                               => "This shouldn't have happened! Report an issue if it DID happen anyway!"
                });
        }

        public class DataSources 
        {
            public RedisTable<AccessToken> AccessTokens => RedisTable<AccessToken>.Connect("AccessToken::", TimeSpan.FromMinutes(30));
            public RedisTable<User> Users => RedisTable<User>.Connect("Users::", null);
            //Redis is perhaps not the best place to store users, but this is an example :)
        }

        [StronglyApiedRoot(DataModel.JSON)]
        public class AccessToken
        {
            [StronglyApiedString()]
            public string username;

            [StronglyApiedString()]
            public string permissions;
        }

        [StronglyApiedRoot(DataModel.JSON)]
        public class User
        {
            [StronglyApiedString()]
            public string password; //Of course, you should be hashing your passwords actually store passwords in plain text.

            [StronglyApiedString()]
            public string permissions;
        }
    }
}
