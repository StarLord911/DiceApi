using AutoMapper;
using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.ApiModels;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.ApiReqRes.Tower;
using DiceApi.Data.Data.Tower;
using DiceApi.Data.Requests;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Implements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Contracts
{
    public interface ITowerGameService
    {
        Task<CreateTowerGameResponce> Create(CreateTowerGameRequest request);

        Task<OpenTowerCellResponce> OpenCell(OpenTowerCellRequest request);


        Task<FinishTowerGameResponce> FinishGame(GetByUserIdRequest request);

        Task<TowerGameApiModel> GetActiveTowerGameByUserId(GetByUserIdRequest request);
    }

    public class TowerGameService : ITowerGameService
    {
        Dictionary<int, Dictionary<int, double>> coefficients = new Dictionary<int, Dictionary<int, double>>
        {
            { 1, new Dictionary<int, double>
                {
                    { 1, 1.2 },
                    { 2, 1.5 },
                    { 3, 1.87 },
                    { 4, 2.34 },
                    { 5, 2.92 },
                    { 6, 3.66 },
                    { 7, 4.57 },
                    { 8, 5.72 },
                    { 9, 7.15 },
                    { 10, 8.94 }
                }
            },
            { 2, new Dictionary<int, double>
                {
                    { 1, 1.6 },
                    { 2, 2.66 },
                    { 3, 4.44 },
                    { 4, 7.4 },
                    { 5, 12.34 },
                    { 6, 20.57 },
                    { 7, 34.29 },
                    { 8, 57.15 },
                    { 9, 95.25 },
                    { 10, 158.76 }
                }
            },
            { 3, new Dictionary<int, double>
                {
                    { 1, 2.4 },
                    { 2, 6 },
                    { 3, 15 },
                    { 4, 37.5 },
                    { 5, 93.75 },
                    { 6, 234.37 },
                    { 7, 585.93 },
                    { 8, 1464.84 },
                    { 9, 3662.1 },
                    { 10, 9155.27 }
                }
            },
            { 4, new Dictionary<int, double>
                {
                    { 1, 4.8 },
                    { 2, 24 },
                    { 3, 120 },
                    { 4, 600 },
                    { 5, 3000 },
                    { 6, 15000 },
                    { 7, 75000 },
                    { 8, 375000 },
                    { 9, 1875000 },
                    { 10, 9375000 }
                }
            }
        };

        private readonly ICacheService _cacheService;
        private readonly IUserService _userService;
        private readonly IAntiMinusService _antiMinusService;
        private readonly IWageringRepository _wageringRepository;
        private readonly ILastGamesService _lastGamesService;

        public TowerGameService(ICacheService cacheService,
           IUserService userService,
           IAntiMinusService antiMinusService,
           IWageringRepository wageringRepository,
           ILastGamesService lastGamesService)
        {
            _cacheService = cacheService;
            _userService = userService;
            _antiMinusService = antiMinusService;
            _wageringRepository = wageringRepository;

            _lastGamesService = lastGamesService;
        }

        public async Task<CreateTowerGameResponce> Create(CreateTowerGameRequest request)
        {
            var settingsCache = await _cacheService.ReadCache<Settings>(CacheConstraints.SETTINGS_KEY);

            var activeGame = await _cacheService.ReadCache<TowerActiveGame>(CacheConstraints.TOWER_KEY + request.UserId);
            if (activeGame != null && activeGame.IsActiveGame())
            {
                return new CreateTowerGameResponce
                {
                    Info = "Игра уже существует.",
                    Succes = false
                };
            }

            if (!settingsCache.MinesGameActive)
            {
                return new CreateTowerGameResponce
                {
                    Info = "Игра отключена",
                    Succes = false
                };
            }

            var user = _userService.GetById(request.UserId);

            if (request.MinesCount > 4 || request.MinesCount < 1)
            {
                return new CreateTowerGameResponce() { Succes = false, Info = $"Change mines count. Mines count = {request.MinesCount}" };
            }

            if (user.Ballance <= request.Sum)
            {
                return new CreateTowerGameResponce() { Succes = false, Info = "Низкий баланс" };
            }

            if (!user.IsActive)
            {
                return new CreateTowerGameResponce() { Succes = false, Info = "Вы заблокированы" };
            }

            if (request.Sum < 1 || request.Sum > 100  )
            {
                return new CreateTowerGameResponce() { Succes = false, Info = "Ставка должна быть от 1 до 1000" };
            }

            await UpdateWageringAsync(request.UserId, request.Sum);


            await UpdateWageringAsync(request.UserId, request.Sum);

            var game = new TowerActiveGame(request.MinesCount);
            game.BetSum = request.Sum;
            game.UserId = request.UserId;
            game.Chances = coefficients[game.MinesCount];

            await _cacheService.WriteCache(CacheConstraints.TOWER_KEY + request.UserId, game);

            await _userService.UpdateUserBallance(request.UserId, user.Ballance - request.Sum);

            return new CreateTowerGameResponce() { Succes = true, Info = "Game created" };
        }

        public async Task<TowerGameApiModel> GetActiveTowerGameByUserId(GetByUserIdRequest request)
        {
            var serializedGame = await _cacheService.ReadCache<TowerActiveGame>(CacheConstraints.TOWER_KEY + request.UserId);

            if (serializedGame == null)
            {
                return null;
            }

            if (!serializedGame.IsActiveGame())
            {
                return null;
            }

            return new TowerGameApiModel()
            {
                Floors = MapCells(serializedGame.GetCells()),
                MinesCount = serializedGame.MinesCount,
                BetSum = serializedGame.BetSum
            };

        }

        private List<Floor> MapCells(List<List<TowerCell>> cells)
        {
            var res = new List<Floor>();
            foreach (var item in cells)
            {
                var floor = new Floor();

                floor.FloorId = item.FirstOrDefault().Floor;
                floor.IsOpen = item.Any(c => c.IsOpen);

                if (floor.IsOpen)
                {
                    floor.OpenedCellId = item.FirstOrDefault(c => c.IsOpen == true).Position;
                }

                res.Add(floor);
            }

            return res;
        }

        public async Task<OpenTowerCellResponce> OpenCell(OpenTowerCellRequest request)
        {
            var game = await _cacheService.ReadCache<TowerActiveGame>(CacheConstraints.TOWER_KEY + request.UserId);
            
            if (game == null || !game.IsActiveGame())
            {
                return new OpenTowerCellResponce()
                {
                    Message = "Game not found",
                    Succes = false
                };
            }

            // Open the cell in the game
            var openResult = game.OpenCell(request.CellId);

            if (openResult.ThisFloorOpened)
            {
                return new OpenTowerCellResponce
                {
                    Succes = true,
                    Message = "This floor opened",
                    Result = new OpenCellResult
                    {
                        FindMine = false,
                        GameOver = false,
                        ThisFloorOpened = true
                    }
                };
            }

            if (openResult.FindMine)
            {
                return await HandleMineFound(game, request);
            }

            if (openResult.GameOver)
            {
                await SendNewGameSocket(game);
                openResult.Cells = SerializationHelper.Serialize(game.GetCells());

                await _cacheService.DeleteCache(CacheConstraints.TOWER_KEY + request.UserId);
                return new OpenTowerCellResponce
                {
                    Succes = true,
                    Message = "Game win",
                    Result = openResult
                };
            }

            await UpdateGameInCache(game, request.UserId);

            return new OpenTowerCellResponce
            {
                Succes = true,
                Message = "Game continue",
                Result = openResult
            };
        }

        public async Task<FinishTowerGameResponce> FinishGame(GetByUserIdRequest request)
        {
            var game = await _cacheService.ReadCache<TowerActiveGame>(CacheConstraints.TOWER_KEY + request.UserId);

            if (game == null)
            {
                return new FinishTowerGameResponce { Succes = false, Message = "Game not found" };
            }

            if (game.TowerFloor == 1)
            {
                return new FinishTowerGameResponce { Succes = false, Message = "Игру нельзя завершить" };
            }

            await _cacheService.DeleteCache(CacheConstraints.TOWER_KEY + request.UserId);
            var user = _userService.GetById(request.UserId);
            await _userService.UpdateUserBallance(request.UserId, user.Ballance + game.CanWin);

            return new FinishTowerGameResponce { Succes = true, Cells = SerializationHelper.Serialize(game.GetCells()), UserBallance = user.Ballance + game.CanWin };
        }

        private async Task<OpenTowerCellResponce> HandleMineFound(TowerActiveGame game, OpenTowerCellRequest request)
        {
            await SaveGameAndDeleteCache(game, request.UserId);
            await SendNewGameSocket(game);

            return new OpenTowerCellResponce
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

        private async Task SendNewGameSocket(TowerActiveGame game, string userName = null)
        {
            if (userName == null)
            {
                userName = _userService.GetById(game.UserId).Name;
            }

            await _lastGamesService.AddLastGames(userName, game.BetSum, Math.Round(game.CanWin, 2), game.FinishGame, GameType.Tower);
        }

        private async Task SaveGameAndDeleteCache(TowerActiveGame game, long userId)
        {
            //var mappedGame = _mapper.Map<MinesGame>(game);
            //await _minesRepository.AddMinesGame(mappedGame);
            await _cacheService.DeleteCache(CacheConstraints.TOWER_KEY + userId);
        }


        private async Task UpdateWageringAsync(long userId, decimal sum)
        {
            var wagering = await _wageringRepository.GetActiveWageringByUserId(userId);

            if (wagering != null && wagering.IsActive)
            {
                await _wageringRepository.UpdatePlayed(userId, sum);

                if (wagering.Wagering < wagering.Played + sum)
                {
                    await _wageringRepository.DeactivateWagering(wagering.Id);
                }
            }
        }

        private async Task UpdateGameInCache(TowerActiveGame game, long userId)
        {
            await _cacheService.UpdateCache(CacheConstraints.TOWER_KEY + userId, game);
        }
    }
}
