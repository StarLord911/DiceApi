using DiceApi.Common;
using DiceApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services
{
    public static class FakeActiveHelper
    {
        public static List<string> FakeNames = new List<string>()
        {
            "buran102",
            "musa09",
            "CSPER",
            "Markissa",
            "ivanko93",
            "ioppooi",
            "Artem12",
            "XeQtR",
            "azzz11",
            "akumko",
            "777qqq",
            "GANG",
            "Dragoon",
            "MadG1k",
            "Aza1124",
            "ufc2000",
            "PPSIXexe",
            "1gaame",
            "DOGEeee",
            "SAtoshi1",
            "xleb77",
            "melstroy",
            "Stark23",
            "Fenix",
            "Adam1",
            "AUYE8",
            "volk1",
            "azazaza1",
            "mynamq1",
            "MagA06",
            "RUSTEAM1",
            "rustam6",
            "neo12",
            "kamikadze",
            "pohu2",
            "anyaa1",
            "mustafa",
            "Ella12",
            "dolchevita",
            "misha1",
            "Annnn",
            "xermus",
            "oll123",
            "Buran102",
            "Musa09",
            "CSPER",
            "Markissa",
            "Ivanko93",
            "Ioppooi",
            "Artem12",
            "XeQtR",
            "Azzz11",
            "Akumko",
            "777qqq",
            "GANG",
            "Dragoon",
            "MadG1k",
            "Aza1124",
            "Ufc2000",
            "Ppsixexe",
            "1gaame",
            "Dogeeee",
            "Satoshi1",
            "Xleb77",
            "Melstroy",
            "Stark23",
            "Fenix",
            "Adam1",
            "Auye8",
            "Volk1",
            "Azazaza1",
            "Mynamq1",
            "MagA06",
            "Rusteam1",
            "Rustam6",
            "Neo12",
            "Kamikadze",
            "Pohu2",
            "Anyaa1",
            "Mustafa",
            "Ella12",
            "Dolchevita",
            "Misha1",
            "Annnn",
            "Xermus",
            "Oll123"
        };

        public static int FakeUserCount = 0;
        public static int UserCount = 1;

        public static Dictionary<int, List<double>> GetChanses()
        {
            var dic = new Dictionary<int, List<double>>
            {
                { 2, new List<double>() { 1.09, 1.19, 1.3, 1.43, 1.58, 1.75, 1.96, 2.21, 2.5, 2.86, 3.3, 3.85, 4.55, 5.45, 6.67, 8.33, 10.71, 14.29, 20, 30, 50, 100, 300 } },
                { 3, new List<double>() { 1.14, 1.3, 1.49, 1.73, 2.02, 2.37, 2.82, 3.38, 4.11, 5.05, 6.32, 8.04, 10.45, 13.94, 19.17, 27.38, 41.07, 65.7, 115, 230, 575, 2300 } },
                { 4, new List<double>() { 1.19, 1.43, 1.73, 2.11, 2.61, 3.26, 4.13, 5.32, 6.95, 9.27, 12.64, 17.69, 25.56, 38.33, 60.24, 100.4, 180.71, 361.43, 843.33, 2530, 12650 } },
                { 5, new List<double>() { 1.25, 1.58, 2.02, 2.61, 3.43, 4.57, 6.2, 8.59, 12.16, 17.69, 26.54, 41.28, 67.08, 115, 210.83, 421.67, 948.75, 2530, 8855, 53130 } },
                { 6, new List<double>() { 1.32, 1.75, 2.37, 3.26, 4.57, 6.53, 9.54, 14.31, 22.12, 35.38, 58.97, 103.21, 191.67, 383.33, 843.33, 2108.33, 6325, 25300, 177100 } },
                { 7, new List<double>() { 1.39, 1.96, 2.82, 4.13, 6.2, 9.54, 15.1, 24.72, 42.02, 74.7, 140.06, 280.13, 606.94, 1456.67, 4005.83, 13352.78, 60087.5, 480700 } },
                { 8, new List<double>() { 1.47, 2.21, 3.38, 5.32, 8.59, 14.31, 24.72, 44.49, 84.04, 168.08, 360.16, 840.38, 2185, 6555, 24035, 120175, 1081575 } },
                { 9, new List<double>() { 1.56, 2.5, 4.11, 6.95, 12.16, 22.12, 42.02, 84.04, 178.58, 408.19, 1020.47, 2857.31, 9286.25, 37145, 204297.5, 2042975 } }
            };

            return dic;
        }

        //TODO написать феик активность для рулетки и перекинуть внутрь джобы
        public static GameApiModel GetGameApiModel()
        {
            var random = new Random();
            var nameInex = random.Next(0, FakeNames.Count);
            double sum = random.NextDouble() * 99 + 1; // От 1 до 100

            if (random.Next(0, 9) != 2)
            {
                sum = Math.Round(sum, 2);
            }

            var name = FakeNames[nameInex];
            var multiplier = random.NextDouble() * 6 + 1;
            bool win = random.Next(0, 6) > 3;

            var gameType = GameType.DiceGame;

            if (random.Next(0, 10) >= 8)
            {
                multiplier = GetChanses()[random.Next(2, 9)][random.Next(4, 9)];
                gameType = GameType.Mines;
            }

            var canWin = (decimal)(sum * multiplier);

            var apiModel = new GameApiModel
            {
                UserName = ReplaceAt(name, 4, '*'),
                Sum = Math.Round((decimal)sum, 2),
                CanWinSum = Math.Round(canWin, 2),
                Multiplier = Math.Round((decimal)multiplier),
                Win = win,
                GameType = gameType,
                GameDate = DateTime.Now.GetMSKDateTime().ToString("HH:mm")
            };

            return apiModel;
        }

        private static string ReplaceAt(string input, int index, char newChar)
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
