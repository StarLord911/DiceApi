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
        public string UserName { get; set; }

        public string SendDate { get; set; }

        public string Message { get; set; }
    }
}
