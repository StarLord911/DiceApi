using DiceApi.Data.Data.Dice;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DiceApi.Services.Implements
{
    public class DiceService : IDiceService
    {
        private readonly IUserService _userService;
        private readonly IDiceGamesRepository _diceGamesRepository;
        private readonly IWageringRepository _wageringRepository;
        private readonly IPaymentAdapterService _paymentAdapterService;

        private readonly ILogRepository _logRepository;

        private long _maxBet = 10000;

        public DiceService(IUserService userService,
            IDiceGamesRepository diceGamesRepository,
            ILogRepository logRepository,
            IWageringRepository wageringRepository,
            IPaymentAdapterService paymentAdapterService)
        {
            _userService = userService;
            _diceGamesRepository = diceGamesRepository;
            _logRepository = logRepository;
            _wageringRepository = wageringRepository;
            _paymentAdapterService = paymentAdapterService;
        }

        public async Task<DiceResponce> StartDice(DiceRequest request)
        {
            if (!ValidateDiceRequest(request))
            {
                return new DiceResponce { IsSucces = false };
            }

            //отладить надо
            var responce = new DiceResponce();

            var win = (100.0 / request.Persent) * request.Sum;
            var winSum = Math.Round(win, 2);
            var user = _userService.GetById(request.UserId);

            var random = new Random().Next(1, 100);
            var currentBallance = await _paymentAdapterService.GetCurrentBallance();

            //антиминус логика, если наш баланс больше чем ставка то игра играется, иначе игра проигрывается.
            if (currentBallance > request.Sum)
            {
                if (request.Persent > random)
                {
                    responce.IsSucces = true;
                    responce.NewBallance = user.Ballance += (winSum - request.Sum);
                    await _userService.UpdateUserBallance(request.UserId, responce.NewBallance);
                }
                else
                {
                    responce.IsSucces = false;
                    responce.NewBallance = user.Ballance -= request.Sum;
                    await _userService.UpdateUserBallance(request.UserId, responce.NewBallance);
                }
            }
            else
            {
                responce.IsSucces = false;
                responce.NewBallance = user.Ballance -= request.Sum;
                await _userService.UpdateUserBallance(request.UserId, responce.NewBallance);
            }

            

            var diceGame = new DiceGame()
            {
                UserId = request.UserId,
                Persent = request.Persent,
                Sum = request.Sum,
                CanWin = winSum,
                Win = responce.IsSucces,
                GameDateTime = DateTime.Now
            };

            await _diceGamesRepository.Add(diceGame);

            var wagering = await _wageringRepository.GetActiveWageringByUserId(request.UserId);

            if (wagering != null)
            {
                await _wageringRepository.UpdatePlayed(request.UserId, request.Sum);
                
                if (wagering.Wageringed < wagering.Played + request.Sum)
                {
                    await _wageringRepository.DeactivateWagering(wagering.Id);
                }
            }

            await _logRepository.LogInfo("Add new dice game");

            return responce;
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

            if (request.Persent < 1 || request.Persent > 95 || containsUser == null || request.Sum > _maxBet
                 || request.Sum > containsUser.Ballance)
            {
                return false;
            }

            return true;
        }
    }
}