using DiceApi.Data.Data.Dice;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DiceApi.Common;
using DiceApi.Data;

namespace DiceApi.Services.Implements
{
    public class DiceService : IDiceService
    {
        private readonly IUserService _userService;
        private readonly IDiceGamesRepository _diceGamesRepository;
        private readonly IWageringRepository _wageringRepository;
        private readonly IPaymentAdapterService _paymentAdapterService;
        private readonly ICacheService _cacheService;

        private readonly ILogRepository _logRepository;

        public DiceService(IUserService userService,
            IDiceGamesRepository diceGamesRepository,
            ILogRepository logRepository,
            IWageringRepository wageringRepository,
            IPaymentAdapterService paymentAdapterService,
            ICacheService cacheService)
        {
            _userService = userService;
            _diceGamesRepository = diceGamesRepository;
            _logRepository = logRepository;
            _wageringRepository = wageringRepository;
            _paymentAdapterService = paymentAdapterService;
            _cacheService = cacheService;
        }

        public async Task<(DiceResponce, DiceGame)> StartDice(DiceRequest request)
        {
            if (!ValidateDiceRequest(request))
            {
                return (new DiceResponce { IsSucces = false }, new DiceGame());
            }

            //отладить надо
            var responce = new DiceResponce();

            var win = Convert.ToDecimal((100.0 / request.Persent)) * request.Sum;
            var winSum = Math.Round(win, 2);
            var user = _userService.GetById(request.UserId);

            var random = new Random().Next(1, 100);
            var currentBallance = await _paymentAdapterService.GetCurrentBallance();
            var settingsCache = await _cacheService.ReadCache(CacheConstraints.SETTINGS_KEY);
            var cache = SerializationHelper.Deserialize<Settings>(settingsCache);

            //антиминус логика, если наш баланс больше чем ставка то игра играется, иначе игра проигрывается.
            if (cache.DiceAntiminus > request.Sum)
            {
                if (request.Persent > random)
                {
                    responce.IsSucces = true;
                    responce.NewBallance = (user.Ballance += (winSum - request.Sum));
                    await _userService.UpdateUserBallance(request.UserId, responce.NewBallance);
                    cache.DiceAntiminus -= winSum;

                }
                else
                {
                    responce.IsSucces = false;
                    responce.NewBallance = user.Ballance -= request.Sum;
                    await _userService.UpdateUserBallance(request.UserId, responce.NewBallance);
                    cache.DiceAntiminus += request.Sum;
                }
            }
            else
            {
                responce.IsSucces = false;
                responce.NewBallance = user.Ballance -= request.Sum;
                await _userService.UpdateUserBallance(request.UserId, responce.NewBallance);

                cache.DiceAntiminus += request.Sum;
            }
           
            var newCache = SerializationHelper.Serialize(cache);
            await _cacheService.DeleteCache(CacheConstraints.SETTINGS_KEY);
            await _cacheService.WriteCache(CacheConstraints.SETTINGS_KEY, newCache);

            var diceGame = new DiceGame()
            {
                UserId = request.UserId,
                Persent = request.Persent,
                Sum = request.Sum,
                CanWin = winSum,
                Win = responce.IsSucces,
                GameTime = DateTime.Now
            };

            await _diceGamesRepository.Add(diceGame);

            var wagering = await _wageringRepository.GetActiveWageringByUserId(request.UserId);

            if (wagering != null)
            {
                await _wageringRepository.UpdatePlayed(request.UserId, request.Sum);
                
                if (wagering.Wagering < wagering.Played + request.Sum)
                {
                    await _wageringRepository.DeactivateWagering(wagering.Id);
                }
            }

            await _logRepository.LogInfo("Add new dice game");

            return (responce, diceGame);
        }

        public async Task<List<DiceGame>> GetLastGames()
        {
            return await _diceGamesRepository.GetLastGames();
        }

        public async Task<List<DiceGame>> GetAllDiceGamesByUserId(long userId)
        {
            return await _diceGamesRepository.GetByUserId(userId);
        }

        private bool ValidateDiceRequest(DiceRequest request)
        {
            var containsUser = _userService.GetById(request.UserId);

            if (request.Persent < 1 || request.Persent > 95 || containsUser == null || request.Sum > containsUser.Ballance)
            {
                return false;
            }

            return true;
        }
    }
}