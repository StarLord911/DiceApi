﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Games
{
    /// <summary>
    /// Games
    /// </summary>
    public class GameModel
    {
        /// <summary>
        /// ID игры
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Какой юзер играл
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Ставка
        /// </summary>
        public decimal BetSum { get; set; }

        /// <summary>
        /// Победил или проиграл
        /// </summary>
        public bool Win { get; set; }

        /// <summary>
        /// Возможный выигрыш.
        /// </summary>
        public decimal CanWin { get; set; }

        /// <summary>
        /// Время когда была игры
        /// </summary>
        [Column("gameTime")]
        public DateTime GameTime { get; set; }

        public GameType GameType { get; set;}
    }
}
