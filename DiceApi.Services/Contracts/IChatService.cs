using DiceApi.Data.Data.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Contracts
{
    public interface IChatService
    {
        Task AddChatMessage(ChatMessage chatMessage, bool isFake = false);

        Task<List<ChatMessage>> GetAllChatMessages();
    }
}
