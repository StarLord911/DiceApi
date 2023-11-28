using AutoMapper;
using DiceApi.Attributes;
using DiceApi.Data;
using DiceApi.Data.Admin;
using DiceApi.Data.Api.Model;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Admin;
using DiceApi.Data.Data.Payment;
using DiceApi.Data.Dice;
using DiceApi.Data.Payments;
using DiceApi.Data.Requests;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DiceApi.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IPaymentService _paymentService;
        private readonly IWithdrawalsService _withdrawalsService;
        private readonly IDiceService _diceService;
        private readonly IPromocodeActivationHistory _promocodeActivationHistory;
        private readonly IPaymentRequisitesRepository _paymentRequisitesRepository;

        public AdminController(IUserService userService,
            IPaymentService paymentService,
            IWithdrawalsService withdrawalsService,
            IDiceService diceService,
            IPromocodeActivationHistory promocodeActivationHistory,
            IPaymentRequisitesRepository paymentRequisitesRepository,
            IMapper mapper)
        {
            _userService = userService;
            _paymentService = paymentService;
            _withdrawalsService = withdrawalsService;
            _diceService = diceService;
            _promocodeActivationHistory = promocodeActivationHistory;
            _paymentRequisitesRepository = paymentRequisitesRepository;
            _mapper = mapper;
        }

        [Authorize(true)]
        [HttpPost("getUsersByPage")]
        public async Task<List<AdminUserInfo>> GetUsersByPage(GetUsersByPaginationRequest request)
        {
            var users = await _userService.GetUsersByPagination(request);

            return users.Select(u => _mapper.Map<AdminUserInfo>(u)).ToList();
        }

        [Authorize(true)]
        [HttpPost("getMainPageStats")]
        public async Task<AdminMainPageStats> GetMainPageStats()
        {
            var result = new AdminMainPageStats();

            result.PaymentStats = await _paymentService.GetPaymentStats();
            result.WithdrawalStats = await _withdrawalsService.GetWithdrawalStats();
            return result;
        }

        //[Authorize(true)]
        [HttpPost("getUserById")]
        public async Task<AdminUser> GetUserById(GetByUserIdRequest request)
        {
            var user = _userService.GetById(request.Id);

            var mappedUser = _mapper.Map<AdminUser>(user);

            mappedUser.AuthIpAddres = "";
            mappedUser.RegistrationIpAddres = "";

            mappedUser.BallanceAdd = (await _paymentService.GetPaymentsByUserId(user.Id)).Where(p => p.Status == PaymentStatus.Payed).Sum(s => s.Amount);
            mappedUser.ExitBallance = (await _withdrawalsService.GetAllActiveByUserId(user.Id)).Where(w => !w.IsActive).Sum(s => s.Amount);
            //TODO: допилить еще для маинса
            var diceGames = (await _diceService.GetAllDiceGamesByUserId(user.Id));

            mappedUser.EarnedMoney = diceGames.Where(d => !d.Win).Sum(s => s.Sum) - diceGames.Where(d => d.Win).Sum(s => s.CanWin);
            mappedUser.ReffsAddedBallance = await GetReffsAddBallance(user.Id);
            mappedUser.ReffsExitBallance = await GetReffsExitBallance(user.Id);
            mappedUser.RefferalCount = (await _userService.GetRefferalsByUserId(user.Id)).Count;

            return mappedUser;
        }

        //[Authorize(true)]
        [HttpPost("getPromocodeActivateHistory")]
        public async Task<PaginatedList<PrimocodeActivation>> GetPromocodeActivateHistory(PromocodeActivateHisoryRequest request)
        {
            var history = await _promocodeActivationHistory.GetPromocodeActivatesByUserId(request.Id);
            var pageCount = DivideWithCeiling(history.Count, request.PageSize);

            if (request.PageNumber == 1)
            {
                return new PaginatedList<PrimocodeActivation>(history.Take(request.PageSize).ToList(), pageCount, request.PageNumber);
            }

            if (pageCount == 0)
            {
                return new PaginatedList<PrimocodeActivation>(history, pageCount, request.PageNumber);
            }

            var result = history.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList();

            return new PaginatedList<PrimocodeActivation>(result, pageCount, request.PageNumber);
        }

        //[Authorize(true)]
        [HttpPost("getGamesStats")]
        public async Task<GamesStatsResponce> GetGamesStats(GetByUserIdRequest request)
        {
            var diceGames = await _diceService.GetAllDiceGamesByUserId(request.Id);

            var result = new GamesStatsResponce();

            result.DiceAllBetsSum = diceGames.Sum(d => d.Sum);
            result.DiceBetCount = diceGames.Count;
            result.DiceLoseSum = diceGames.Where(d => !d.Win).Sum(s => s.Sum);
            result.DiceWinSum = diceGames.Where(d => d.Win).Sum(s => s.CanWin);
            //TODO: допилить для маинса
            return result;
        }

        //[Authorize(true)]
        [HttpPost("getGamesByUserId")]
        public async Task<PaginatedList<GameApiModel>> GetGamesByUserId(GetGamesByUserIdByPaginationRequest request)
        {
            var games = await _diceService.GetAllDiceGamesByUserId(request.Id);
            var diceGames = games
                .OrderByDescending(d => d.GameTime)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            //TODO: допилить для маинса
            var totalItemCount = games.Count;
            var totalPages = (int)Math.Ceiling((double)totalItemCount / request.PageSize);
            var mappedGames = new List<GameApiModel>();

            foreach (var game in diceGames)
            {
                var user = _userService.GetById(game.UserId);

                var apiModel = new GameApiModel
                {
                    UserName = user.Name,
                    Sum = game.Sum,
                    CanWinSum = game.CanWin,
                    Multiplier = Math.Round(game.CanWin / game.Sum, 2),
                    GameDate = game.GameTime,
                    GameType = GameType.DiceGame
                };

                mappedGames.Add(apiModel);
            }

            return new PaginatedList<GameApiModel>(mappedGames, totalPages, request.PageNumber);
        }

        //[Authorize(true)]
        [HttpPost("getPaymantRequisitesByUserId")]
        public async Task<PaymentRequisite> GetPaymantRequisitesByUserId(GetByUserIdRequest request)
        {
            return await _paymentRequisitesRepository.GetPaymentRequisiteByUserId(request.Id);
        }

        //[Authorize(true)]
        [HttpPost("getPaymantsByUserId")]
        public async Task<List<Payment>> GetPaymantsByUserId(GetByUserIdRequest request)
        {
            return await _paymentService.GetPaymentsByUserId(request.Id);
        }

        //[Authorize(true)]
        [HttpPost("getWithdrawalsByUserId")]
        public async Task<List<Withdrawal>> GetWithdrawalsByUserId(GetByUserIdRequest request)
        {
            return await _withdrawalsService.GetAllByUserId(request.Id);
        }

        private int DivideWithCeiling(int dividend, int divisor)
        {
            int result = dividend / divisor;
            if (dividend % divisor != 0)
            {
                result += 1;
            }
            return result;
        }

        private async Task<decimal> GetReffsAddBallance(long userId)
        {
            var referals = await _userService.GetRefferalsByUserId(userId);
            decimal result = 0;

            foreach (var referal in referals)
            {
                result += (await _paymentService.GetPaymentsByUserId(referal.Id)).Where(p => p.Status == PaymentStatus.Payed).Sum(s => s.Amount);
            }

            return result;
        }

        private async Task<decimal> GetReffsExitBallance(long userId)
        {
            var referals = await _userService.GetRefferalsByUserId(userId);
            decimal result = 0;

            foreach (var referal in referals)
            {
                result += (await _withdrawalsService.GetAllActiveByUserId(referal.Id)).Where(p => p.IsActive).Sum(s => s.Amount);
            }

            return result;
        }

    }
}
