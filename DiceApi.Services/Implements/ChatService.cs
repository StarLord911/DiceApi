using DiceApi.Common;
using DiceApi.Data.Data.Chat;
using DiceApi.Services.Contracts;
using DiceApi.Services.SignalRHubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
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
        private readonly IUserService _userService;
        private readonly IHubContext<ChatMessagesHub> _chatMessagesHub;

        public ChatService(ICacheService cacheService, IUserService userService,
            IHubContext<ChatMessagesHub> chatMessagesHub)
        {
            _cacheService = cacheService;
            _userService = userService;
            _chatMessagesHub = chatMessagesHub;
        }

        public async Task AddChatMessage(ChatMessage chatMessage)
        {
            var user = await _userService.GetUserByName(chatMessage.UserName);

            if (user == null)
            {
                throw new Exception("User cannot find");
            }
            
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


            await _chatMessagesHub.Clients.All.SendAsync("ReceiveMessage", JsonConvert.SerializeObject(chatMessage));

            await _cacheService.WriteCache(CacheConstraints.CHAT_MESSAGES, messages);
        }

        public async Task<List<ChatMessage>> GetAllChatMessages()
        {
            return await _cacheService.ReadCache<List<ChatMessage>>(CacheConstraints.CHAT_MESSAGES);
        }
    }
}
