using DiceApi.Attributes;
using DiceApi.Data.Data.Chat;
using DiceApi.Services.Common;
using DiceApi.Services.Contracts;
using DiceApi.Services.SignalRHubs;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace DiceApi.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatMessagesHub> _hubContext;

        public ChatController(IChatService chatService,
            IHubContext<ChatMessagesHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        [Authorize]
        [HttpPost("addNewMessage")]
        public async Task AddNewMessage(ChatMessage chatMessage)
        {
            try
            {
                chatMessage.UserName = ReplaceAt(chatMessage.UserName, 4, '*');
                //TODO переделать, нужно брать юзера из бд и имя у юзера
                await _chatService.AddChatMessage(chatMessage);

                var chatJson = JsonConvert.SerializeObject(chatMessage);

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", chatJson);
            }
            catch
            {

            }
        }

        [HttpPost("getMessages")]
        public async Task<List<ChatMessage>> GetMessages()
        {
            return await _chatService.GetAllChatMessages();
        }

        private string ReplaceAt(string input, int index, char newChar)
        {
            if (index < 0 || index >= input.Length)
            {
                return input;
            }

            char[] chars = input.ToCharArray();
            for (int i = index; i < input.Length; i++)
            {
                chars[i] = newChar;
            }
            return new string(chars);
        }
    }
}
