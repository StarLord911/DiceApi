using DiceApi.Common;
using DiceApi.Data.ApiReqRes.Thimble;
using DiceApi.Data.Data.Games;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.DataAcces.Repositoryes.Game;
using DiceApi.Services.Contracts;
using MathNet.Numerics.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Implements.Thimbles
{
    public interface IThimblesService
    {
        Task<BetThimblesResponce> Bet(BetThimblesRequest request);
    }

    public class ThimblesService : IThimblesService
    {
        private readonly IGamesRepository _gamesRepository;
        private readonly IUserService _userService;
        private readonly IWageringService _wageringService;

        public ThimblesService(IGamesRepository gamesRepository,
            IUserService userService)
        {
            _gamesRepository = gamesRepository;
            _userService = userService;
        }

        public async Task<BetThimblesResponce> Bet(BetThimblesRequest request)
        {
            var user = _userService.GetById(request.UserId);

            if (user.Ballance < request.BetSum)
            {
                return new BetThimblesResponce()
                {
                    Message = "Нехватает денег.",
                    Succes = false,
                    Win = false
                };
            }

            if (request.BetSum > 1000)
            {
                return new BetThimblesResponce()
                {
                    Message = "Максимальная ставка 1000.",
                    Succes = false,
                    Win = false
                };
            }

            await _wageringService.UpdatePlayed(user.Id, request.BetSum);

            await _userService.UpdateUserBallance(request.UserId, user.Ballance - request.BetSum);

            var game = new GameModel()
            {
                UserId = user.Id,
                BetSum = request.BetSum,
                CanWin = request.BetSum * 3,
                GameType = Data.GameType.Thimbles,
                GameTime = DateTime.UtcNow.GetMSKDateTime()
            };

            var randoom = new MersenneTwister().Next(1, 3);

            if (randoom == 1)
            {
                await _userService.UpdateUserBallance(request.UserId, user.Ballance + (request.BetSum * 3));

                game.Win = true;
                await _gamesRepository.AddGame(game);
                
                return new BetThimblesResponce()
                {
                    Message = $"Вы выиграли {Math.Round(request.BetSum * 3, 2)}",
                    Succes = true,
                    Win = true
                };
            }

            game.Win = false;
            await _gamesRepository.AddGame(game);

            return new BetThimblesResponce()
            {
                Message = $"Вы проиграли",
                Succes = true,
                Win = false
            };

        }

    }
}
