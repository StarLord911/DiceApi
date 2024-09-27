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
        private readonly IMinesRepository _minesRepository;
        private readonly IAntiMinusService _antiMinusService;
        private readonly IWageringRepository _wageringRepository;
        private readonly ILastGamesService _lastGamesService;

        public TowerGameService(ICacheService cacheService,
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

            _lastGamesService = lastGamesService;
        }

        public async Task<CreateTowerGameResponce> Create(CreateTowerGameRequest request)
        {
            var settingsCache = await _cacheService.ReadCache<Settings>(CacheConstraints.SETTINGS_KEY);

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
                return new CreateTowerGameResponce() { Succes = false, Info = "Lack of balance" };
            }

            if (!user.IsActive)
            {
                return new CreateTowerGameResponce() { Succes = false, Info = "User blocked" };
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
        

        public Task<OpenTowerCellResponce> OpenCell(OpenTowerCellRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<FinishTowerGameResponce> FinishGame(GetByUserIdRequest request)
        {
            var game = await _cacheService.ReadCache<TowerActiveGame>(CacheConstraints.TOWER_KEY + request.UserId);

            if (game == null || !game._gameOver)
            {
                return new FinishTowerGameResponce { Succes = false, Message = "Game not found" };
            }

            if (game.TowerFloor == 0)
            {
                return new FinishTowerGameResponce { Succes = false, Message = "Игру нельзя завершить" };
            }

            await _cacheService.DeleteCache(CacheConstraints.MINES_KEY + request.UserId);

            var user = _userService.GetById(request.UserId);
            await _userService.UpdateUserBallance(request.UserId, user.Ballance + game.CanWin);
            game.FinishGame = true;

            var settingsCache = await _cacheService.ReadCache<Settings>(CacheConstraints.SETTINGS_KEY);

            settingsCache.MinesGameWinningSettings.MinesAntiminusBallance -= game.CanWin;

            await _cacheService.UpdateCache(CacheConstraints.SETTINGS_KEY, settingsCache);


            return new FinishTowerGameResponce { Cells = SerializationHelper.Serialize(game.GetCells()), UserBallance = user.Ballance + game.CanWin };
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
    }
}
