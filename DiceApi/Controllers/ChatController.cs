using DiceApi.Attributes;
using DiceApi.Data.Data.Chat;
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
    }
}
