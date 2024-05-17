using DiceApi.Common;
using DiceApi.Data.Data.Chat;
using DiceApi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Implements
{
    public class ChatService : IChatService
    {
        private readonly ICacheService _cacheService;

        public ChatService(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task AddChatMessage(ChatMessage chatMessage)
        {
            var messages = await _cacheService.ReadCache<List<ChatMessage>>(CacheConstraints.CHAT_MESSAGES);
            await _cacheService.DeleteCache(CacheConstraints.CHAT_MESSAGES);

            if (messages == null)
            {
                messages = new List<ChatMessage>() { chatMessage };

                await _cacheService.WriteCache(CacheConstraints.CHAT_MESSAGES, messages);
            }
            else
            {
                messages.Add(chatMessage);
            }

            if (messages.Count >= 50)
            {
                messages.RemoveAt(0);
            }

            await _cacheService.WriteCache(CacheConstraints.CHAT_MESSAGES, messages);
        }

        public async Task<List<ChatMessage>> GetAllChatMessages()
        {
            return await _cacheService.ReadCache<List<ChatMessage>>(CacheConstraints.CHAT_MESSAGES);
        }
    }
}
