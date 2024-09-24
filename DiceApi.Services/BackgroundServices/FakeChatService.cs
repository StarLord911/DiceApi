using DiceApi.Common;
using DiceApi.Data.Data.Chat;
using DiceApi.Services.Common;
using DiceApi.Services.Contracts;
using DiceApi.Services.SignalRHubs;
using MathNet.Numerics.Random;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiceApi.Services.BackgroundServices
{
    public class FakeChatService : BackgroundService
    {
        private List<string> _messages = new List<string>()
        {
            "игра дает?",
            "кому тактики нужны?",
            "когда выидет новый промо?",
            "админ, платеж не пришел",
            "выпало зеро х18",
            "кому скинуть промо?",
            "когда добавлят слоты?",
            "-3к за 2мин",
            "Разбаньте в телеграмме",
            "Как выйти с аккаунта?",
            "больше 2 часов вывод в ожидании, это что за дела?",
            "Здравствуйте, что делать если пишет, ведите корректный кошелек?",
            "Парни а с фк на карту долгий вывод?",
            "Дайте промокод пж",
            "ЗАНОСЫ ЗАНОСЫ ЗАНОСЫ",
            "где ЗАНОСЫ!!!",
            "админ дай балланс пжжжжж",
            "одни бичи в чате",
            "Можете сказать как сделать реферальную ссылку?",
            "Админ можно процент по рефке поднять??",
            "Забаньте его он промо хочет дать с телеги",
            "Новый промик в телеге",
            "Пацаны кто больше 150к делал могу промо и на 1111 руб дать?",
            "Саит дает",
            "+11к за 10 мин, админ выводи быстрее))",
            "Душ не баня, Помыл яйца, до свидания!",
            "Настоящий джентльмен пропустит Даму вперёд, чтобы посмотреть на неё сзади",
            "Если мужик сказал, значит он это сделает, и не надо ему каждые пол года об этом напоминать",
            "Некоторые думают, что они поднялись. На самом деле, они просто всплыли на время.",
            "стетхем где?",
            "вывод за 15 мин, я в шоке",
            "че за дичь тут происходит",
            "Салам лудикам....",
            "Сам ты лудик",
            "АДМИНН",
            "БЛЯТЬ эта храмая лошадь бегать умеет?",
            "А что не так с лошадью",
            "бет проебал просто, скрипт",
            "Где где но тут точно скриптов нет",
            "Скрипты есть на любом сайте",
            "Я тут давно, тут нет скриптов",
            "как добавят проверку игры увидим есть скрипты или нет",
            "ребят есть промо?",
            "промики только в официаьной группе...",
            "кто то выиграл сегодня?",
            "+7к на маинсе, играть не умеете просто",
            "какой играть, тут все от удачи зависит",
            "++",
            "--",
            "Сегодня наконец удача на моей стороне",
            "че так",
            "Победил поэтому",
            "Лудоманы идите лечитесь",
            "Иди ты нахер лушче",
            "++",
            "любыми схемами обнуть эту храмую лошадь",
            "Это моя ЛОШАДЬ",
            "АаХАХАХАХ",
            "максимум на ишаке будешь кататься",
            "На ишаке тоесть на тебе?",
            "ЗАебали хуйню писать",
            "не спамьте пж"

        };

        private readonly IChatService _chatService;
        private readonly IHubContext<ChatMessagesHub> _hubContext;

        public FakeChatService(IChatService chatService,
            IHubContext<ChatMessagesHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }
       
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (true)
                {
                    foreach (var item in _messages)
                    {
                        var random = new MersenneTwister();
                        var nameInex = random.Next(0, FakeActiveHelper.FakeNames.Count);

                        var chatMessage = new ChatMessage()
                        {
                            Message = item,
                            SendDate = DateTime.Now.GetMSKDateTime().ToString("HH:mm"),
                            UserName = FakeActiveHelper.FakeNames[nameInex]
                        };

                        //TODO переделать, нужно брать юзера из бд и имя у юзера
                        await _chatService.AddChatMessage(chatMessage, true);

                        var chatJson = JsonConvert.SerializeObject(chatMessage);

                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", chatJson);
                        
                        await Task.Delay(new TimeSpan(0,random.Next(1, 3), random.Next(1, 59)));
                    }

                }

            }
            catch
            {

            }
        }
    }
}
