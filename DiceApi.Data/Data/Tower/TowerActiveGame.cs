﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Tower
{
    public class TowerActiveGame
    {
        [DataMember]
        public List<List<TowerCell>> _cells;

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
        public Dictionary<int, double> Chances { get; set; }

        [DataMember]
        public decimal CanWin { get; set; }

        public int TowerFloor { get; set; }

        public TowerActiveGame(int minsCount)
        {
            TowerFloor = 1;
            _cells = GenerateMineField(minsCount);
            _gameOver = false;
            MinesCount = minsCount;
        }

        public List<List<TowerCell>> GetCells()
        {
            return _cells;
        }

        public bool IsActiveGame()
        {
            return !_gameOver;
        }

        private List<List<TowerCell>> GenerateMineField(int mineCount)
        {
            var random = new Random();
            List<List<TowerCell>> mineField = new List<List<TowerCell>>();

            for (int i = 1; i <= 10; i++)
            {
                List<TowerCell> floor = new List<TowerCell>();

                for (int j = 1; j <= 5; j++)
                {
                    floor.Add(new TowerCell(i, j));
                }

                mineField.Add(floor);
            }

            int minesPlaced = 0;

            foreach (var floor in mineField)
            {
                while (minesPlaced < mineCount)
                {
                    int x = random.Next(1, 5);

                    if (!floor.FirstOrDefault(c => c.Position == x).IsMined)
                    {
                        floor.FirstOrDefault(c => c.Position == x).IsMined = true;
                        minesPlaced++;
                    }
                }
                minesPlaced = 0;
            }
           

            return mineField;
        }

        public OpenCellResult OpenCell(int position)
        {
            if (_gameOver)
            {
                return new OpenCellResult { GameOver = true, IsCellOpened = false, FindMine = false };
            }

            var cells = _cells[TowerFloor - 1];

            if (cells.Any(c => c.IsOpen))
            {
                return new OpenCellResult { GameOver = false, IsCellOpened = false, FindMine = false, ThisFloorOpened = true };
            }

            var cell = cells.FirstOrDefault(f => f.Position == position);

            if (cell.IsOpen)
            {
                return new OpenCellResult { GameOver = false, IsCellOpened = true, FindMine = false };
            }

            if (cell.IsMined)
            {
                CanWin = 0;
                _gameOver = true;
                return new OpenCellResult { GameOver = true, IsCellOpened = false, FindMine = true };
            }

            Chances.TryGetValue(TowerFloor, out var chanse);

            CanWin = (decimal)chanse * BetSum;
            cell.IsOpen = true;
            TowerFloor += 1;
            return new OpenCellResult { CanWin = CanWin, GameOver = false, IsCellOpened = false, FindMine = false };
        }
    }

    public class TowerCell
    {
        [DataMember]
        public int Floor { get; }

        [DataMember]
        public int Position { get; }

        [DataMember]
        public bool IsMined { get; set; }

        [DataMember]
        public bool IsOpen { get; set; }

        public TowerCell(int floor, int pos)
        {
            Floor = floor;
            Position = pos;
            IsMined = false;
            IsOpen = false;
        }
    }
}
