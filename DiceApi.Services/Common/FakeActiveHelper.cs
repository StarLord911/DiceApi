using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.ApiReqRes.HorseRace;
using DiceApi.Data.Data.Roulette;
using MathNet.Numerics.Random;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services
{
    public static class FakeActiveHelper
    {
        public static List<string> FakeNames = new List<string>()
        {
            "Alexey", "Maria", "Ivan", "Ekaterina", "Dmitry",
            "Pavel", "Svetlana", "Oleg", "Natalya", "Anna",
            "Igor", "Yuri", "Grigory", "Tatiana", "Victor",
            "Mikhail", "Valeria", "Sergey", "Olga", "Elena",
            "Andrey", "Timur", "Irina", "Ruslan", "Timofey",
            "Valentin", "Fyodor", "Stanislav", "Lyubov", "Valentin",
            "Lydia", "Alexander", "Nikolay", "Evgeny", "Arthur",
            "Alyona", "Kirill", "Alla", "Eduard", "Makar",
            "Denis", "Lera", "Zahar", "Kristina", "Sofia",
            "Vasilisa", "Yakov", "Elvira", "Margarita", "Nikita",
            "Agatha", "Zhanna", "Zhenya", "Roman", "Vladislav",
            "Inessa", "Matvey", "Danil", "Arina", "Yaroslav",
            "Anton", "Nadezhda", "Daniil", "Vladimir", "Pyotr",
            "Lyudmila", "Vera", "Tamara", "Zoya", "Ksenia",
            "Vsevolod", "Efim", "Lev", "Marina", "Artem",
            "Philip",
            "александр", "мария", "наталья", "петр", "екатерина",
            "олег", "света", "игорек", "лена", "аня",
            "саша", "маша", "юля", "миша", "вадим",
            "катя", "оленька", "дениска", "тёма", "костя",
            "ваня", "слава", "даниил", "фаина", "илья",
            "алекс", "александра", "антошка", "василий", "гена",
            "ляля", "саня", "фёдор", "вадимка", "костян",
            "вован", "юрка", "ромка", "тимка", "виталя",
            "артёмка", "сюзанна", "гриша", "стасик", "андрюша",
            "левка", "мирон", "влад", "нагель", "кира",
            "вилена", "жора", "руслан", "николаша", "митя",
            "филя", "тимоша", "славик", "иванна", "лилия",
            "матюша", "сема", "лариса", "даниилка", "никола",
            "самсон", "виталий", "эгор", "яночка", "степан",
            "алина", "андрюха", "петька", "степашка", "константин",
            "васена", , "весна", "егор", "марианна",
            "Ulya2000",
            "Sanya98",
            "Larisa2017",
            "Victor2015rus",
            "Ksyusha89",
            "Bogdan05rus",
            "Diana1996",
            "Rita2011",
            "Stepan84",
            "Yura10rus",
            "Zhanna2008",
            "Lesha2014",
            "Ignat91rus",
            "Alisa1985",
            "Zinaida2023",
            "Artur03",
            "Inna2002rus",
            "Gena88",
            "Filipp19",
            "Sonya2009",
            "Arina80rus",
            "Igor2006",
            "Galya97",
            "Kirill2018rus",
            "Lyudmila2001",
            "Susanna95",
            "Isabella2010",
            "Mark03rus",
            "Yakov98",
            "Miron2022",
            "Petya1993",
            "Kristina86rus",
            "Vasily13",
            "Emma2013",
            "Sima97rus",
            "Roman18",
            "Zhena91",
            "Mila2007",
            "Borislava00rus",
            "Jenya2019",
            "Julia93",
            "Lev2025",
            "Aliya2016rus",
            "Ivan1985",
            "Mia99",
            "Filat2024",
            "Emil1987rus",
            "Varvara94",
            "Daniil2023",
            "Amina2018rus",
            "Slava02rus",
            "Eva2003",
            "Oscar2011",
            "Korina88rus",
            "Maxim96",
            "Regina2020",
            "Aidar2004",
            "Simona85rus",
            "Damir09",
            "Elizaveta2005",
            "Zakhar2019rus",
            "Tonya2012",
            "Anfisa89",
            "Vsevolod2014",
            "Larissa2008rus",
            "Semyon2017",
            "Karina92",
            "Boris2021rus",
            "Timofey87",
            "Lola13rus",
            "Kir1995",
            "Egor007",
            "Vlad2008",
            "Sergo95rus",
            "Dima89rus",
            "Anna1995",
            "Olya2020",
            "Nikita17rus",
            "Masha2001",
            "Kolya007",
            "Sasha93",
            "Nastya2012",
            "Ira88rus",
            "Andrey67",
            "Artem12rus",
            "Katya99",
            "Tanya2003",
            "Pavel96rus",
            "Vika2010",
            "Ivan1987",
            "Gleb13",
            "Svetlana2004",
            "Misha2015rus",
            "Alina90",
            "Roma05",
            "Yulia2018",
            "Maksim14rus",
            "Dasha08",
            "Sergey2011",
            "Alyona92",
            "Kostya2007",
            "Lena2017",
            "Vitaly01rus",
            "Nina06",
            "Vera1989",
            "Vadim2019",
            "Oleg95rus",
            "Denis03",
            "Polina1998",
            "Zhenya2016",
            "Tima90rus",
            "Kira2000",
            "Fedya07",
            "Grisha2002",
            "Tonya94rus",
            "Igor1986",
            "Liza04",
            "Vanya19rus",
            "Slava1988",
            "Valya2006",
            "Timur2013",
            "Luda97rus",
            "Kostya85",
            "Oksana12",
            "Vitya2005",
            "Marina1991rus",
            "Stas2021",
            "Ilya94",
            "Yana2009",
            "Ruslan99rus",
            "Lyuba87",
            "Fedor2016",
            "Elena92rus",
            "Vadim2002",
            "Roman100",
            "Ulya14rus",
            "Anatoly01",
            "Larisa2018",
            "Gosha96rus",
            "Kirill75",
            "Zoya2010",
            "Daniil1990",
            "Taisiya03",
            "Bagrat99rus",
            "Nikita02",
            "Evgeny84",
            "Orest2007",
            "Inga2011rus",
            "Ratmir88",
            "Liza2001",
            "Max2015",
            "Semyon96rus",
            "Olga99",
            "Platon97",
            "Evgeniya11",
            "Yaroslav19rus",
            "Stepan06",
            "Artur07rus",
            "Irina1995",
            "Ksyu2022",
            "Anton85rus",
            "Tatiana93",
            "Diana2013rus",
            "Raisa1989",
            "Petya05",
            "Galya2020",
            "Artur98rus",
            "Mila87",
            "Filip03",
            "Yakov2004",
            "Yurik1985",
            "Bogdan2009",
            "Alina92",
            "Sasha2007",
            "Milana08rus",
            "David96",
            "Kseniya12",
            "Vika2016",
            "Zahar98rus",
            "Sofiya2010",
            "Mariya96",
            "Galina1997",
            "Sergey85rus",
            "Varvara00",
            "Rozalia88",
            "Vladislav94",
            "Leonid1991rus",
            "Nadya04",
            "Serafima07",
            "Tamara15rus",
            "Anya03",
            "Sima2006",
            "Gennady99rus",
            "Slava01",
            "Murat2002",
            "Rimma2019",
            "Metodius84rus",
            "Kuzma90",
            "Sima2012",
            "Timofey94",
            "Alena2001rus",
            "Angel21",
            "Vova2014",
            "Dmitry98",
            "Marianna88rus",
            "Iskra2003",
            "Emilia08",
            "Julian1987",
            "Yuriy2010rus",
            "Olesya95",
            "Ruben07",
            "Boris2001",
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
            "Oll123",

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

        public static List<decimal> RandomDigits = new List<decimal>()
        {
            15, 20, 50, 70, 60, 45,75, 44,58,96,53, 99, 10,150, 120, 180, 200, 300, 350, 323, 425, 400, 458,
            500, 412,88,77,45,12,5,11,9,6,1,1,1,21,22,12,15,47,56,78,21,49,56,44,89,98,878,78,56,15,35,48,46,12,35,77,55,69,63,62,61,
            21, 89, 34, 67, 128, 45, 93, 12, 56, 78,
            90, 45, 23, 67, 101, 145, 87, 39, 122, 32,
            46, 68, 77, 98, 123, 141, 52, 64, 110, 85,
            38, 59, 105, 129, 53, 44, 71, 94, 102, 37,
            61, 86, 112, 139, 25, 72, 88, 106, 33, 149,
            58, 81, 114, 47, 135, 19, 65, 107, 28, 74,
            142, 99, 36, 50, 91, 119, 140, 27, 83, 48,
            15, 63, 108, 124, 43, 75, 133, 20, 55, 132,
            31, 84, 146, 13, 69, 125, 92, 29, 100, 60,
            137, 40, 11, 104, 131, 2, 97, 16, 66, 113,

            7, 87, 130, 42, 57, 103, 76, 95, 134, 30,
            49, 121, 110, 17, 73, 24, 19, 143, 35, 68,
            120, 139, 26, 54, 92, 137, 41, 80, 118, 47,
            136, 22, 66, 115, 33, 72, 144, 107, 39, 62,

            10, 84, 29, 56, 1, 41, 18, 6, 88, 19,
            49, 67, 12, 132, 21, 98, 115, 63, 48, 117,
            16, 16, 9, 14, 53, 80, 145, 32, 71, 12,
            95, 38, 6, 12, 33, 12, 77, 106, 20, 61,

            10, 84, 29, 56, 1, 41, 18, 6, 88, 19,
            49, 67, 12, 132, 21, 98, 115, 63, 48, 117,
            16, 16, 9, 14, 53, 80, 145, 32, 71, 12,
            95, 38, 6, 12, 33, 12, 77, 106, 20, 61,
            10, 84, 29, 56, 1, 41, 18, 6, 88, 19,
            49, 67, 12, 132, 21, 98, 115, 63, 48, 117,
            16, 16, 9, 14, 53, 80, 145, 32, 71, 12,
            95, 38, 6, 12, 33, 12, 77, 106, 20, 61,
            10, 84, 29, 56, 1, 41, 18, 6, 88, 19,
            49, 67, 12, 132, 21, 98, 115, 63, 48, 117,
            16, 16, 9, 14, 53, 80, 145, 32, 71, 12,
            95, 38, 6, 12, 33, 12, 77, 106, 20, 61,
            10, 84, 29, 56, 1, 41, 18, 6, 88, 19,
            49, 67, 12, 132, 21, 98, 115, 63, 48, 117,
            16, 16, 9, 14, 53, 80, 145, 32, 71, 12,
            95, 38, 6, 12, 33, 12, 77, 106, 20, 61,
            10, 84, 29, 56, 1, 41, 18, 6, 88, 19,
            49, 67, 12, 132, 21, 98, 115, 63, 48, 117,
            16, 16, 9, 14, 53, 80, 145, 32, 71, 12,
            95, 38, 6, 12, 33, 12, 77, 106, 20, 61,
            10, 84, 29, 56, 1, 41, 18, 6, 88, 19,
            49, 67, 12, 132, 21, 98, 115, 63, 48, 117,
            16, 16, 9, 14, 53, 80, 145, 32, 71, 12,
            95, 38, 6, 12, 33, 12, 77, 106, 20, 61,
            10, 84, 29, 56, 1, 41, 18, 6, 88, 19,
            49, 67, 12, 132, 21, 98, 115, 63, 48, 117,
            16, 16, 9, 14, 53, 80, 145, 32, 71, 12,
            95, 38, 6, 12, 33, 12, 77, 106, 20, 61,

            107, 34, 10, 55, 117, 64, 83, 135, 22, 90,
            137, 50, 79, 124, 103, 29, 112, 148, 11, 58,
            93, 141, 36, 73, 126, 45, 96, 139, 17, 52,

            131, 28, 81, 144, 114, 46, 108, 65, 99, 86,
            143, 31, 75, 92, 137, 59, 12, 104, 128, 39,
            60, 119, 27, 98, 84, 125, 151, 14, 69, 133,
            44, 70, 115, 8, 55, 87, 136, 23, 61, 101,

            147, 33, 92, 111, 138, 19, 57, 116, 74, 130,
            25, 66, 105, 82, 142, 43, 90, 123, 31, 64,
            149, 37, 99, 113, 144, 58, 26, 77, 132, 45,

            89, 127, 18, 71, 103, 50, 19, 47, 83, 28,

            138, 54, 121, 62, 93, 18, 10, 34, 79, 130,
            101, 16, 49, 112, 91, 14, 44, 123, 67, 38,
            105, 152, 22, 88, 135, 46, 74, 134, 17, 61,
            119, 39, 70, 128, 13,163, 56, 97, 14, 14, 5,
            25, 89, 112, 30, 82, 137, 49, 68, 16, 45,
            111, 155, 64, 12, 99, 54, 17, 107, 91, 15,
            26, 70, 129, 42, 76, 118, 35, 109, 13,2,
            20, 85, 143, 51, 95, 124, 38, 68, 111, 28,
            82, 14, 56, 11, 136, 47, 79, 125, 32, 64,
            141, 59, 102, 14, 15, 87, 18, 25, 67, 119,

            94, 36, 159, 48, 71, 143, 57, 81, 147, 34,
            63, 114, 53, 96, 19, 24, 110, 12, 45, 98,
            14, 125, 131, 23, 75, 56, 45, 34, 118, 90,
            101, 15, 130, 47, 132, 81, 61, 39, 140, 45,
            140, 2, 137, 49, 87, 120, 70, 91, 87, 58, 12, 116, 10, 10, 85, 35, 18, 14, 75, 124, 114, 18, 15,
            16, 79, 69, 67, 129, 128, 3, 132, 96, 10, 34, 29, 62, 42, 38, 131, 51, 9, 54, 45, 40, 31, 17, 6, 
            144, 38, 19, 90, 149, 55, 64, 100, 77, 13, 78, 90, 107, 79, 87, 86, 1, 19, 75, 96, 64, 16, 131, 22, 133,
            90, 60, 134, 11, 112, 139, 16, 55, 18, 82, 76, 11, 20, 79, 17, 76, 4, 25, 59, 99, 30, 71, 50, 79, 95, 
            1, 96, 33, 31, 77, 9, 99, 70, 91, 129, 85, 147, 97, 43, 68, 139, 138, 8, 101, 54, 8, 110, 24, 48, 80, 94, 
            35, 14, 23, 50, 50, 21, 70, 97, 118, 39, 137, 27, 139, 94, 108, 95, 146, 63, 57, 33, 19, 62, 33, 140, 10, 8, 
            7, 3, 35, 84, 41, 77, 30, 50, 37, 51, 120, 4, 119, 133, 63, 13, 130, 11, 18, 70, 67, 54, 103, 72, 23, 141, 
            10, 10, 14, 5, 118, 28, 11, 41, 45, 16, 92, 105, 12, 39, 93, 78, 75, 103, 16, 37, 28, 45, 12, 107, 18, 39,
            10, 39, 16, 102, 149.03m, 132.57m, 22.09m, 4.58m, 1.38m, 54.58m, 95.48m, 73.23m, 28.62m,
            16.25m, 139.12m, 133.64m, 69.1m,
            139.87m, 11.23m, 108.12m, 8.15m, 3.44m, 100.53m, 107.14m, 145.89m, 141.72m, 55.98m, 60.21m, 94.85m, 58.44m, 102.59m, 96.94m, 134.08m,
            91.33m, 110.78m, 117.54m, 112.15m, 28.36m, 10.41m, 111.84m, 61.13m, 10.69m, 52.4m, 3.28m, 57.93m, 133.3m, 82.25m, 92.91m, 115.75m, 103.44m,
            79.82m, 133.97m, 5.66m, 118.78m, 74.65m, 15.47m, 64.56m, 83.55m, 124.02m, 18.96m, 141.41m, 3.15m, 6.96m, 58.61m, 108.29m, 87.12m, 101.46m,
            148.81m, 31.98m, 43.39m, 52.56m, 51.74m, 146.54m, 98.59m, 61.36m, 63.55m, 89.97m, 39.64m, 145.25m, 133.47m, 136.89m, 104.64m, 49.24m, 65.56m,
            128.98m, 19.81m, 82.7m, 118.22m, 64.12m, 83.75m, 25.6m, 126.32m, 108.53m, 75.89m, 147.64m, 38.23m, 9.67m, 14.29m, 71.17m, 142.08m, 141.88m, 77.96m, 78.92m, 112.3m
        };

        //TODO написать феик активность для рулетки и перекинуть внутрь джобы
        public static GameApiModel GetMinesGameApiModel()
        {
            var random = new MersenneTwister();
            var nameInex = random.Next(0, FakeNames.Count);
            var sum = RandomDigits[random.Next(0, RandomDigits.Count)];

            if (random.Next(0, 9) != 2)
            {
                sum = Math.Round(sum, 2);
            }

            var name = FakeNames[nameInex];
            var multiplier = (decimal)GetChanses()[random.Next(2, 9)][random.Next(2, 9)];
            var randWin = random.Next(0, 1);
            bool win = randWin == 1;
            var gameType = GameType.Mines;

            var apiModel = new GameApiModel
            {
                UserName = ReplaceAt(name, 4, '*'),
                Sum = Math.Round(sum, 2),
                Multiplier = Math.Round(multiplier, 2),
                Win = win,
                GameType = gameType,
                GameDate = DateTime.Now.GetMSKDateTime().ToString("HH:mm")
            };

            return apiModel;
        }

        public static GameApiModel GetDiceGameApiModel()
        {
            var random = new MersenneTwister();
            var nameInex = random.Next(0, FakeNames.Count);
            var sum = RandomDigits[random.Next(0, RandomDigits.Count)];

            if (random.Next(0, 9) != 2)
            {
                sum = Math.Round(sum, 2);
            }

            var name = FakeNames[nameInex];
            var multiplier = random.Next(1, 5) + random.NextDecimal();
            var randWin = random.Next(0, 1);

            bool win = randWin == 1;
            var gameType = GameType.DiceGame;
            var apiModel = new GameApiModel
            {
                UserName = ReplaceAt(name, 4, '*'),
                Sum = Math.Round(sum, 2),
                Multiplier = Math.Round(multiplier, 2),
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

        public static List<RouletteActiveBet> FakeRouletteActiveBet = new List<RouletteActiveBet>();

        public static List<HorseRaceActiveBet> FakeHorseRaceActiveBet = new List<HorseRaceActiveBet>();


    }
}
