using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public class UserRegisterResponce
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("ownerId")]
        public long? OwnerId { get; set; }
    }

    public class UserTelegramRegisterResponce
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("ownerId")]
        public long? OwnerId { get; set; }

        public long TelegramUserId { get; set; }

    }

    public class IsTelegramUserRegistredRequest
    {
        public long TelegramUserId { get; set; }
    }
    
}
