using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.User.Api.Requests
{
    public class GetRefferalStatsByUserIdRequest
    {
        [JsonProperty("userId")]
        public long UserId { get; set; }

        [JsonProperty("dateRange")]
        public DateRange DateRange { get; set; }
    }

    public enum DateRange 
    {
        AllTime = 1,
        Month = 2,
        Week = 3,
        Day = 4

    }
}
