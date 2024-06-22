using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Chat
{
    /// <summary>
    /// Содержит инфу о сообщений из чата.
    /// </summary>
    public class ChatMessage
    {
        [JsonProperty("userName")]
        public string UserName { get; set; }
        
        [JsonProperty("sendDate")]
        public string SendDate { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
