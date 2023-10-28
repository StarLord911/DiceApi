using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Api.Model
{
    public class UserApi
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ballance")]
        public double Ballance { get; set; }

        [JsonProperty("ownerId")]
        public long? OwnerId { get; set; }

        [JsonProperty("registrationDate")]
        public DateTime RegistrationDate { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }
    }
}
