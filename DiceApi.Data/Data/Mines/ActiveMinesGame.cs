﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    /// <summary>
    /// Класс содержит инфу о активной игре в маинс
    /// </summary>
    [DataContract]
    public class ActiveMinesGame
    {
        [DataMember]
        public int OpenedCellsCount { get; set; }
        [DataMember]
        public List<Cell> _cells;
        [DataMember]
        public bool _gameOver;
        public bool FinishGame { get; set; }
        [DataMember]
        public int MinesCount { get; set; }
        [DataMember]
        public decimal BetSum { get; set; }
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public List<double> Chances { get; set; }
        [DataMember]
        public decimal CanWin { get; set; }

        [DataMember]
        public int StreamerBonus { get; set; }


        public ActiveMinesGame(int minsCount)
        {
            OpenedCellsCount = 0;
            _cells = GenerateMineField(minsCount);
            _gameOver = false;
            MinesCount = minsCount;
        }

        public bool IsActiveGame()
        {
            return !_gameOver;
        }

        private List<Cell> GenerateMineField(int mineCount)
        {
            Random random = new Random();
            List<Cell> mineField = new List<Cell>();

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    mineField.Add(new Cell(i, j));
                }
            }

            int minesPlaced = 0;

            while (minesPlaced < mineCount)
            {
                int x = random.Next(0, 5);
                int y = random.Next(0, 5);

                if (!mineField.FirstOrDefault(c => c.X == x && c.Y == y).IsMined)
                {
                    mineField.FirstOrDefault(c => c.X == x && c.Y == y).IsMined = true;
                    minesPlaced++;
                }
            }

            return mineField;
        }

        public OpenCellResult OpenCell(int x, int y)
        {
            if (_gameOver)
            {
                OpenedCellsCount++;
                return new OpenCellResult { GameOver = true, IsCellOpened = false, FindMine = false };
            }

            var cell = _cells.FirstOrDefault(c => c.X == x && c.Y == y);

            if (cell.IsOpen)
            {
                return new OpenCellResult { GameOver = false, IsCellOpened = true, FindMine = false };
            }

            if (cell.IsMined)
            {
                OpenedCellsCount++;
                CanWin = 0;
                _gameOver = true;
                return new OpenCellResult { GameOver = true, IsCellOpened = false, FindMine = true };
            }

            var canWin = (decimal)Chances[OpenedCellsCount] * BetSum;
            CanWin = canWin;
            OpenedCellsCount++;
            cell.IsOpen = true;

            return new OpenCellResult { CanWin = CanWin, GameOver = false, IsCellOpened = false, FindMine = false };
        }

        public List<Cell> GetCells()
        {
            return _cells;
        }
    }

    public class Cell
    {
        [DataMember]
        public int X { get; }

        [DataMember]
        public int Y { get; }

        [DataMember]
        public bool IsMined { get; set; }

        [DataMember]
        public bool IsOpen { get; set; }

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
            IsMined = false;
            IsOpen = false;
        }
    }
}
