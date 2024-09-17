using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    /// <summary>
    /// Запрос на создание ставки на скачки.
    /// </summary>
    public class CreateHorseBetRequest
    {
        public long UserId { get; set; }

        public List<HorseBet> HorseBets { get; set; }
    }

    /// <summary>
    /// Ставка на конкретную лошадь.
    /// </summary>
    public class HorseBet
    {
        public decimal BetSum { get; set; }

        public HorseColor HorseColor { get; set; }
    }

    /// <summary>
    /// Цвета лошадей.
    /// </summary>
    public enum HorseColor
    {
        Violet = 1,
        Blue = 2,
        LightGreen = 3,
        Orange = 4,
        Red = 5,
        Green = 6,
        Yellow = 7,
        Beige = 8
    }

    public class CreateHorseBetResponce
    {
        public bool Succes { get; set; }
        public string Message { get; set; }
    }
}
