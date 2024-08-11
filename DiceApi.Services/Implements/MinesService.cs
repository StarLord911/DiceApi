using AutoMapper;
using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.ApiModels;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Winning;
using DiceApi.Data.Requests;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Contracts;
using DiceApi.Services.SignalRHubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DiceApi.Services
{
    public class MinesService : IMinesService
    {
        private readonly ICacheService _cacheService;
        private readonly IUserService _userService;
        private readonly IMinesRepository _minesRepository;
        private readonly IAntiMinusService _antiMinusService;
        private readonly IWageringRepository _wageringRepository;
        private readonly ILastGamesService _lastGamesService;

        private readonly Dictionary<int, List<double>> chanses = new Dictionary<int, List<double>>
        {
            { 2, new List<double>() { 1.09, 1.19, 1.3, 1.43, 1.58, 1.75, 1.96, 2.21, 2.5, 2.86, 3.3, 3.85, 4.55, 5.45, 6.67, 8.33, 10.71, 14.29, 20, 30, 50, 100, 300 } },
            { 3, new List<double>() { 1.14, 1.3, 1.49, 1.73, 2.02, 2.37, 2.82, 3.38, 4.11, 5.05, 6.32, 8.04, 10.45, 13.94, 19.17, 27.38, 41.07, 65.7, 115, 230, 575, 2300 } },
            { 4, new List<double>() { 1.19, 1.43, 1.73, 2.11, 2.61, 3.26, 4.13, 5.32, 6.95, 9.27, 12.64, 17.69, 25.56, 38.33, 60.24, 100.4, 180.71, 361.43, 843.33, 2530, 12650 } },
            { 5, new List<double>() { 1.25, 1.58, 2.02, 2.61, 3.43, 4.57, 6.2, 8.59, 12.16, 17.69, 26.54, 41.28, 67.08, 115, 210.83, 421.67, 948.75, 2530, 8855, 53130 } },
            { 6, new List<double>() { 1.32, 1.75, 2.37, 3.26, 4.57, 6.53, 9.54, 14.31, 22.12, 35.38, 58.97, 103.21, 191.67, 383.33, 843.33, 2108.33, 6325, 25300, 177100 } },
            { 7, new List<double>() { 1.39, 1.96, 2.82, 4.13, 6.2, 9.54, 15.1, 24.72, 42.02, 74.7, 140.06, 280.13, 606.94, 1456.67, 4005.83, 13352.78, 60087.5, 480700 } },
            { 8, new List<double>() { 1.47, 2.21, 3.38, 5.32, 8.59, 14.31, 24.72, 44.49, 84.04, 168.08, 360.16, 840.38, 2185, 6555, 24035, 120175, 1081575 } },
            { 9, new List<double>() { 1.56, 2.5, 4.11, 6.95, 12.16, 22.12, 42.02, 84.04, 178.58, 408.19, 1020.47, 2857.31, 9286.25, 37145, 204297.5, 2042975 } },
            { 10, new List<double>() { 1.67, 2.86, 5.05, 9.27, 17.69, 35.38, 74.7, 168.08, 408.19, 1088.5, 3265.49, 11429.23, 49526.67, 297160, 3268760 } },
            { 11, new List<double>() { 1.79, 3.3, 6.32, 12.64, 26.54, 58.97, 140.06, 360.16, 1020.47, 3265.49, 12245.6, 57146.15, 371450, 4457400 } },
            { 12, new List<double>() { 1.92, 3.85, 8.04, 17.69, 41.28, 103.21, 280.13, 840.38, 2857.31, 11429.23, 57146.14, 400023.08, 5200300 } },
            { 13, new List<double>() { 2.08, 4.55, 10.45, 25.56, 67.08, 191.67, 606.94, 2185, 9286.25, 49526.67, 371450, 5200300 } },
            { 14, new List<double>() { 2.27, 5.45, 13.94, 38.33, 115, 383.33, 1456.67, 6555, 37145, 297160, 4457400 } },
            { 15, new List<double>() { 2.5, 6.67, 19.17, 60.24, 210.83, 843.33, 4005.83, 24035, 204297.5, 3268760 } },
            { 16, new List<double>() { 2.78, 8.33, 27.38, 100.4, 421.67, 2108.33, 13352.78, 120175, 2042975 } },
            { 17, new List<double>() { 3.13, 10.71, 41.07, 180.71, 948.75, 6325, 60087.5, 1081575 } },
            { 18, new List<double>() { 3.57, 14.29, 65.71, 361.43, 2530, 25300, 480700 } },
            { 19, new List<double>() { 4.17, 20, 115, 843.33, 8855, 177100 } },
            { 20, new List<double>() { 5, 30, 230, 2530, 53130 } },
            { 21, new List<double>() { 6.25, 50, 575, 12650 } },
            { 22, new List<double>() { 8.33, 100, 2300 } },
            { 23, new List<double>() { 12.5, 300 } },
            { 24, new List<double>() { 25 } }
        };

        private readonly IMapper _mapper;

        public MinesService(ICacheService cacheService,
            IUserService userService,
            IMinesRepository minesRepository,
            IAntiMinusService antiMinusService,
            IWageringRepository wageringRepository,
            IMapper mapper,
            ILastGamesService lastGamesService)
        {
            _cacheService = cacheService;
            _userService = userService;
            _minesRepository = minesRepository;
            _antiMinusService = antiMinusService;
            _wageringRepository = wageringRepository;

            _mapper = mapper;
            _lastGamesService = lastGamesService;
        }
        #region public methods

        /// <summary>
        /// Метод создания игры в маинс
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CreateMinesGameResponce> CreateMinesGame(CreateMinesGameRequest request)
        {
            var user = _userService.GetById(request.UserId);

            if (request.MinesCount > 24 || request.MinesCount < 2)
            {
                return new CreateMinesGameResponce() { Succes = false, Info = $"Change mines count. Mines count = {request.MinesCount}" };
            }

            if (user.Ballance <= request.Sum)
            {
                return new CreateMinesGameResponce() { Succes = false, Info = "Lack of balance" };
            }

            if (!user.IsActive)
            {
                return new CreateMinesGameResponce() { Succes = false, Info = "User blocked" };
            }

            var activeGame = await _cacheService.ReadCache<ActiveMinesGame>(CacheConstraints.MINES_KEY + request.UserId);

            if (activeGame != null)
            {
                if (!activeGame.IsActiveGame())
                {
                    await _cacheService.DeleteCache(CacheConstraints.MINES_KEY + request.UserId);
                }
                else
                {
                    return new CreateMinesGameResponce() { Succes = false, Info = "Game already created" };
                }
            }

            await UpdateWageringAsync(request.UserId, request.Sum);

            var game = new ActiveMinesGame(request.MinesCount);
            game.BetSum = request.Sum;
            game.UserId = request.UserId;
            game.Chances = chanses[game.MinesCount];

            var serializedGame = SerializationHelper.Serialize(game);

            await _cacheService.WriteCache(CacheConstraints.MINES_KEY + request.UserId, serializedGame);

            await _userService.UpdateUserBallance(request.UserId, user.Ballance - request.Sum);

            return new CreateMinesGameResponce() { Succes = true, Info = "Game created" };
        }

        public async Task<MinesGameApiModel> GetActiveMinesGameByUserId(GetByUserIdRequest request)
        {
            var serializedGame = await _cacheService.ReadCache<ActiveMinesGame>(CacheConstraints.MINES_KEY + request.UserId);

            if (serializedGame == null)
            {
                return null;
            }
            if (!serializedGame.IsActiveGame())
            {
                return null;
            }

            return new MinesGameApiModel() { Cells = MapCells(serializedGame.GetCells()), MinesCount = serializedGame.MinesCount,
                OpenedCount = serializedGame.OpenedCellsCount, BetSum = serializedGame.BetSum };

        }

        public async Task<FinishMinesGameResponce> FinishGame(GetByUserIdRequest request)
        {
            var game = await _cacheService.ReadCache<ActiveMinesGame>(CacheConstraints.MINES_KEY + request.UserId);

            if (game == null || !game.IsActiveGame())
            {
                return new FinishMinesGameResponce { Succes = false, Message = "Game not found" };
            }

            if (game.OpenedCellsCount == 0)
            {
                return new FinishMinesGameResponce { Succes = false, Message = "Игру нельзя завершить" };
            }

            await _cacheService.DeleteCache(CacheConstraints.MINES_KEY + request.UserId);

            var user = _userService.GetById(request.UserId);
            await _userService.UpdateUserBallance(request.UserId, user.Ballance + game.CanWin);
            game.FinishGame = true;

            var settingsCache = await _cacheService.ReadCache<Settings>(CacheConstraints.SETTINGS_KEY);

            settingsCache.MinesGameWinningSettings.MinesAntiminusBallance -= game.CanWin;

            await _cacheService.UpdateCache(CacheConstraints.SETTINGS_KEY, settingsCache);

            var mappedGame = _mapper.Map<MinesGame>(game);
            await _minesRepository.AddMinesGame(mappedGame);

            await UpdateWinningToDay(mappedGame.CanWin);

            await SendNewGameSocket(game, user.Name);

            return new FinishMinesGameResponce { Cells = SerializationHelper.Serialize(game.GetCells()), UserBallance = user.Ballance + game.CanWin };
        }

        public async Task<OpenCellResponce> OpenCell(OpenCellRequest request)
        {
            // Retrieve active game from cache
            var game = await _cacheService.ReadCache<ActiveMinesGame>(CacheConstraints.MINES_KEY + request.UserId);
            if (game == null || !game.IsActiveGame())
            {
                return CreateErrorResponse("Game not found", true);
            }

            // Open the cell in the game
            var openResult = game.OpenCell(request.X, request.Y);
            if (openResult.FindMine)
            {
                return await HandleMineFound(game, request);
            }

            var settings = await _cacheService.ReadCache<Settings>(CacheConstraints.SETTINGS_KEY);

            // Check if the player can win
            if (!_antiMinusService.CheckMinesAntiMinus(game, settings))
            {
                return await HandleMineBalance(game, request, settings);
            }

            // Update game state in cache
            await UpdateGameInCache(game, request.UserId);

            return new OpenCellResponce
            {
                Succes = true,
                Message = "Game continue",
                Result = openResult
            };
        }

        /// <summary>
        /// Получение игр в маинс для пользователя.
        /// </summary>
        public async Task<List<MinesGame>> GetMinesGamesByUserId(long userId)
        {
            return await _minesRepository.GetMinesGamesByUserId(userId);
        }
        #endregion

        #region private methods

        private async Task UpdateWageringAsync(long userId, decimal sum)
        {
            var wagering = await _wageringRepository.GetActiveWageringByUserId(userId);

            if (wagering != null)
            {
                await _wageringRepository.UpdatePlayed(userId, sum);

                if (wagering.Wagering < wagering.Played + sum)
                {
                    await _wageringRepository.DeactivateWagering(wagering.Id);
                }
            }
        }

        private async Task<OpenCellResponce> HandleMineFound(ActiveMinesGame game, OpenCellRequest request)
        {
            var settingsCache = await _cacheService.ReadCache<Settings>(CacheConstraints.SETTINGS_KEY);
            settingsCache.MinesGameWinningSettings.MinesAntiminusBallance += game.BetSum;
            await _cacheService.UpdateCache(CacheConstraints.SETTINGS_KEY, settingsCache);

            game.FinishGame = false;
            await SaveGameAndDeleteCache(game, request.UserId);
            await SendNewGameSocket(game);

            return new OpenCellResponce
            {
                Succes = false,
                Message = "Game over",
                Result = new OpenCellResult
                {
                    Cells = SerializationHelper.Serialize(game.GetCells()),
                    FindMine = true,
                    GameOver = true
                }
            };
        }

        private async Task<OpenCellResponce> HandleMineBalance(ActiveMinesGame game, OpenCellRequest request, Settings settings)
        {
            settings.MinesGameWinningSettings.MinesAntiminusBallance += game.BetSum;

            var cells = game.GetCells();
            bool found = false;

            for (int i = 0; i < 5 && !found; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (cells[i, j].IsMined && !found)
                    {
                        cells[i, j].IsMined = false;
                        found = true;
                        break;  // выход из внутреннего цикла
                    }
                }
                if (found)
                {
                    break;  // выход из внешнего цикла
                }
            }

            cells[request.X, request.Y].IsMined = true;

            await _cacheService.UpdateCache(CacheConstraints.SETTINGS_KEY, settings);
            await SaveGameAndDeleteCache(game, request.UserId);
            await SendNewGameSocket(game);

            return new OpenCellResponce
            {
                Succes = false,
                Message = "Game over",
                Result = new OpenCellResult
                {
                    Cells = SerializationHelper.Serialize(cells),
                    FindMine = true,
                    GameOver = true
                }
            };
        }

        private async Task SaveGameAndDeleteCache(ActiveMinesGame game, long userId)
        {
            var mappedGame = _mapper.Map<MinesGame>(game);
            await _minesRepository.AddMinesGame(mappedGame);
            await _cacheService.DeleteCache(CacheConstraints.MINES_KEY + userId);
        }

        private async Task UpdateGameInCache(ActiveMinesGame game, long userId)
        {
            await _cacheService.UpdateCache(CacheConstraints.MINES_KEY + userId, game);
        }

        private OpenCellResponce CreateErrorResponse(string message, bool gameOver)
        {
            return new OpenCellResponce
            {
                Succes = false,
                Message = message,
                Result = new OpenCellResult { GameOver = gameOver }
            };
        }

        private async Task SendNewGameSocket(ActiveMinesGame game, string userName = null)
        {
            if (userName == null)
            {
                userName = _userService.GetById(game.UserId).Name;
            }

            await _lastGamesService.AddLastGames(userName, game.BetSum, Math.Round(game.CanWin, 2), game.FinishGame, GameType.Mines);
        }

        private async Task UpdateWinningToDay(decimal amount)
        {
            var stats = await _cacheService.ReadCache<WinningStats>(CacheConstraints.WINNINGS_TO_DAY);

            stats.WinningToDay += amount;

            await _cacheService.UpdateCache(CacheConstraints.WINNINGS_TO_DAY, stats);
        }

        private CellApiModel[,] MapCells(Cell[,] cells)
        {
            int rows = cells.GetLength(0);
            int columns = cells.GetLength(1);

            CellApiModel[,] cellApiModels = new CellApiModel[rows, columns];

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    Cell cell = cells[row, column];
                    CellApiModel cellApiModel = _mapper.Map<CellApiModel>(cell);
                    cellApiModels[row, column] = cellApiModel;
                }
            }

            return cellApiModels;
        }

        private string ReplaceAt(string input, int index, char newChar)
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
        #endregion
    }
}
