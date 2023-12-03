using AutoMapper;
using DiceApi.Attributes;
using DiceApi.Data;
using DiceApi.Data.Admin;
using DiceApi.Data.Api.Model;
using DiceApi.Data.ApiModels;
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
        private readonly IPromocodeService _promocodeService;
        private readonly IPaymentAdapterService _paymentAdapterService;
        private readonly ICooperationRequestRepository _cooperationRequestRepository;


        public AdminController(IUserService userService,
            IPaymentService paymentService,
            IWithdrawalsService withdrawalsService,
            IDiceService diceService,
            IPromocodeActivationHistory promocodeActivationHistory,
            IPaymentRequisitesRepository paymentRequisitesRepository,
            IPromocodeService promocodeService,
            IPaymentAdapterService paymentAdapterService,
            ICooperationRequestRepository cooperationRequestRepository,
            IMapper mapper)
        {
            _userService = userService;
            _paymentService = paymentService;
            _withdrawalsService = withdrawalsService;
            _diceService = diceService;
            _promocodeActivationHistory = promocodeActivationHistory;
            _paymentRequisitesRepository = paymentRequisitesRepository;
            _promocodeService = promocodeService;
            _paymentAdapterService = paymentAdapterService;
            _cooperationRequestRepository = cooperationRequestRepository;

            _mapper = mapper;
        }

        #region users
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
        //FindUserByNameRequest

        [HttpPost("updateUserInformation")]
        public async Task UpdateUserInformation(UpdateUserRequest request)
        {
            await _userService.UpdateUserInformation(request);
        }

        [HttpPost("findUserByName")]
        public async Task<PaginatedList<AdminUserInfo>> FindUsersByName(FindUserByNameRequest request)
        {
            var users = await _userService.GetUsersByName(request);

            var result = users.Items.Select(u => _mapper.Map<AdminUserInfo>(u)).ToList();

            return new PaginatedList<AdminUserInfo>(result, users.TotalPages, users.PageIndex);
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
            mappedUser.ExitBallance = (await _withdrawalsService.GetAllActiveByUserId(user.Id)).Where(w => w.Status == WithdrawalStatus.Confirmed).Sum(s => s.Amount);
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
        public async Task<PaginatedList<Payment>> GetPaymantsByUserId(GetPaymentsByUserIdRequest request)
        {
            var payments = await _paymentService.GetPaymentsByUserId(request.UserId);

            var result = payments.Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .ToList();

            var totalItemCount = payments.Count;

            var totalPages = (int)Math.Ceiling((double)totalItemCount / request.Pagination.PageSize);

            return new PaginatedList<Payment>(result, totalPages, request.Pagination.PageNumber);

        }

        //[Authorize(true)]
        [HttpPost("getWithdrawalsByUserId")]
        public async Task<PaginatedList<Withdrawal>> GetWithdrawalsByUserId(GetWithdrawalsByUserId request)
        {
            var withdrawals = await _withdrawalsService.GetAllByUserId(request.UserId);

            var result = withdrawals.Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
               .Take(request.Pagination.PageSize)
               .ToList();

            var totalItemCount = withdrawals.Count;

            var totalPages = (int)Math.Ceiling((double)totalItemCount / request.Pagination.PageSize);

            return new PaginatedList<Withdrawal>(result, totalPages, request.Pagination.PageNumber);

        }
        #endregion

        #region promocodes

        [HttpPost("generatePromocode")]
        public async Task<string> GeneratePromocode(CreatePromocodeRequest request)
        {
            return await _promocodeService.CreatePromocode(request);
        }
        

       [HttpPost("getPromocodes")]
        public async Task<PaginatedList<PromocodeApiModel>> GetPromocodes(GetPromocodesByPaginationRequest request)
        {
            return await _promocodeService.GetPromocodesByPagination(request);
        }

        [HttpPost("getPromocodeByNameByLike")]
        public async Task<PaginatedList<PromocodeApiModel>> GetPromocodeByNameByLike(GetPromocodesByNameRequest request)
        {
            return await _promocodeService.GetPromocodeByNameByLike(request);
        }
        #endregion

        #region withdrawals
        [HttpPost("getWithdrawals")]
        public async Task<PaginatedList<Withdrawal>> GetWithdrawals(GetWithdrawalsRequest request)
        {
            var withdrawals = (await _withdrawalsService.GetAll()).OrderByDescending(w => w.CreateDate).ToList();

            if (request.OnlyActiveWithdrawals)
            {
                withdrawals = withdrawals.Where(w => w.Status == WithdrawalStatus.New).ToList();
            }

            var result = withdrawals.Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
               .Take(request.Pagination.PageSize)
               .ToList();

            var totalItemCount = withdrawals.Count;

            var totalPages = (int)Math.Ceiling((double)totalItemCount / request.Pagination.PageSize);

            return new PaginatedList<Withdrawal>(result, totalPages, request.Pagination.PageNumber);

        }

        [HttpPost("confirmWithdrawal")]
        public async Task ConfirmWithdrawals(WithdrawalRequest request)
        {
            await _withdrawalsService.СonfirmWithdrawal(request.Id);
        }

        [HttpPost("unconfirmWithdrawal")]
        public async Task UnconfirmWithdrawals(WithdrawalRequest request)
        {
            await _withdrawalsService.DeactivateWithdrawal(request.Id);
        }


        #endregion

        #region payments
        [HttpPost("getPayments")]
        public async Task<PaginatedList<Payment>> GetPayments(GetPaymentsRequest request)
        {
            return await _paymentService.GetPaginatedPayments(request);
        }

        [HttpPost("getPaymentWithdrawals")]
        public async Task<PaginatedList<Withdrawal>> GetPaymentWithdrawals(GetPaymentWithdrawalsRequest request)
        {
            return await _withdrawalsService.GetPaginatedWithdrawals(request);
        }

        #endregion

        #region cooperation
        [HttpPost("createCooperationRequest")]
        public async Task CreateCooperationRequest(CooperationRequest request)
        {
            await _cooperationRequestRepository.CreateCooperationRequest(request);
        }

        [HttpPost("getAllCooperationRequest")]
        public async Task<PaginatedList<CooperationRequest>> GetAllCooperationRequest(PaginationRequest request)
        {
            var requests = (await _cooperationRequestRepository.GetAllCooperationRequests()).ToList();

            var result = requests.Skip((request.PageNumber - 1) * request.PageSize)
               .Take(request.PageSize)
               .ToList();

            var totalItemCount = requests.Count;

            var totalPages = (int)Math.Ceiling((double)totalItemCount / request.PageSize);

            return new PaginatedList<CooperationRequest>(result, totalPages, request.PageNumber);
        }

        #endregion

        #region top dwGetUserPaymentWithdrawalInfoByPagination

        [HttpPost("getUserPaymentInfo")]
        public async Task<PaginatedList<UserPaymentInfo>> GetUserPaymentInfo(PaginationRequest request)
        {
             return await _userService.GetUserPaymentInfoByPagination(request);
        }

        [HttpPost("getUserPaymentWithdrawalInfo")]
        public async Task<PaginatedList<UserPaymentWithdrawalInfo>> GetUserPaymentWithdrawalInfo(PaginationRequest request)
        {
            return await _userService.GetUserPaymentWithdrawalInfoByPagination(request);
        }

        #endregion

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
                result += (await _withdrawalsService.GetAllActiveByUserId(referal.Id))
                    .Where(p => p.Status == WithdrawalStatus.New).Sum(s => s.Amount);
            }

            return result;
        }

    }
}
