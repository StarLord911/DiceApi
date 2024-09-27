using System;
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
            TowerFloor = 0;
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
            Random random = new Random();
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

            while (minesPlaced < mineCount)
            {
                int x = random.Next(0, 5);
                int y = random.Next(0, 5);

                //if (!mineField.FirstOrDefault(c => c.Floor == x && c.Position == y).IsMined)
                //{
                //    mineField.FirstOrDefault(c => c.Floor == x && c.Position == y).IsMined = true;
                //    minesPlaced++;
                //}
            }

            return mineField;
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

