using DiceApi.Data.Data.Chat;
using DiceApi.Hubs;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace DiceApi.Controllers
{
    [Route("api/chat")]
    [EnableCors("AllowAll")]
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
        
        [HttpPost("addNewMessage")]
        public async Task AddNewMessage(ChatMessage chatMessage)
        {
            var chatJson = JsonConvert.SerializeObject(chatMessage);

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", chatJson);

            await _chatService.AddChatMessage(chatMessage);
        }

        [HttpPost("getMessages")]
        public async Task<List<ChatMessage>> GetMessages()
        {
            return await _chatService.GetAllChatMessages();
        }
    }
}
