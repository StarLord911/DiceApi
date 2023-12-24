using AutoMapper;
using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.Admin;
using DiceApi.Data.ApiModels;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Admin;
using DiceApi.Data.Payments;
using DiceApi.Data.Requests;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Hubs;
using DiceApi.Services;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
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
        private readonly IWageringRepository _wageringRepository;
        private readonly IMinesService _minesService;
        private readonly ICacheService _cacheService;
        private IHubContext<NewGameHub> _newGameContext;
        private IHubContext<OnlineUsersHub> _onlineContext;

        public AdminController(IUserService userService,
            IPaymentService paymentService,
            IWithdrawalsService withdrawalsService,
            IDiceService diceService,
            IPromocodeActivationHistory promocodeActivationHistory,
            IPaymentRequisitesRepository paymentRequisitesRepository,
            IPromocodeService promocodeService,
            IPaymentAdapterService paymentAdapterService,
            ICooperationRequestRepository cooperationRequestRepository,
            IWageringRepository wageringRepository,
            IMinesService minesService,
            ICacheService cacheService,
            IHubContext<NewGameHub> hubContext,
            IHubContext<OnlineUsersHub> onlineContext,
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
            _wageringRepository = wageringRepository;
            _minesService = minesService;
            _cacheService = cacheService;
            _newGameContext = hubContext;
            _onlineContext = onlineContext;

            _mapper = mapper;
        }

        #region users

        //[Authorize(true)]
        [HttpPost("getUsersByPage")]
        public async Task<PaginatedList<AdminUserInfo>> GetUsersByPage(GetUsersByPaginationRequest request)
        {
            var res = await _userService.GetUsersByPagination(request);

            return new PaginatedList<AdminUserInfo>(res.Items.Select(u => _mapper.Map<AdminUserInfo>(u)).ToList(), res.TotalPages, res.PageIndex);
        }

        //[Authorize(true)]
        [HttpPost("getMainPageStats")]
        public async Task<AdminMainPageStats> GetMainPageStats()
        {
            var result = new AdminMainPageStats();

            result.PaymentStats = await _paymentService.GetPaymentStats();
            result.WithdrawalStats = await _withdrawalsService.GetWithdrawalStats();

            result.WithdrawalWaitSum = await _withdrawalsService.GetWithdrawalWaitSum();
            result.FreeKassaBallance = await _paymentAdapterService.GetCurrentBallance();
            result.UsersCount = await _userService.GetUserCount();
            return result;
        }

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
            var wager = await _wageringRepository.GetActiveWageringByUserId(request.Id);
            var mappedUser = _mapper.Map<AdminUser>(user);

            mappedUser.AuthIpAddres = "";
            mappedUser.RegistrationIpAddres = user.RegistrationIp;

            mappedUser.BallanceAdd = (await _paymentService.GetPaymentsByUserId(user.Id)).Where(p => p.Status == PaymentStatus.Payed).Sum(s => s.Amount);
            mappedUser.ExitBallance = (await _withdrawalsService.GetAllActiveByUserId(user.Id)).Where(w => w.Status == WithdrawalStatus.Confirmed).Sum(s => s.Amount);
            //TODO: допилить еще для маинса
            var diceGames = (await _diceService.GetAllDiceGamesByUserId(user.Id));
            var minesGames = (await _minesService.GetMinesGamesByUserId(user.Id));

            mappedUser.EarnedMoney = (minesGames.Where(d => !d.Win).Sum(s => s.Sum) + diceGames.Where(d => !d.Win).Sum(s => s.Sum)) - (diceGames.Where(d => d.Win).Sum(s => s.CanWin)
                + minesGames.Where(d => !d.Win).Sum(s => s.CanWin));

            mappedUser.ReffsAddedBallance = await GetReffsAddBallance(user.Id);
            mappedUser.ReffsExitBallance = await GetReffsExitBallance(user.Id);
            mappedUser.RefferalCount = (await _userService.GetRefferalsByUserId(user.Id)).Count;
            mappedUser.Wager = wager != null ? wager.Wagering - wager.Played : 0;
            mappedUser.DepositForWithdrawal = 0;
            mappedUser.BallanceInGame = 0;

            var cache = await _cacheService.ReadCache(CacheConstraints.MINES_KEY + request.Id);
            if (cache != null)
            {
                mappedUser.BallanceInGame = SerializationHelper.Deserialize<ActiveMinesGame>(cache).BetSum;
            }


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
            var minesGames = await _minesService.GetMinesGamesByUserId(request.Id);

            var result = new GamesStatsResponce();

            result.DiceAllBetsSum = diceGames.Sum(d => d.Sum);
            result.DiceBetCount = diceGames.Count;
            result.DiceLoseSum = diceGames.Where(d => !d.Win).Sum(s => s.Sum);
            result.DiceWinSum = diceGames.Where(d => d.Win).Sum(s => s.CanWin - s.Sum);

            result.MinesAllBetsSum = minesGames.Sum(d => d.Sum);
            result.MinesBetCount = minesGames.Count;
            result.MinesLoseSum = minesGames.Where(d => !d.Win).Sum(s => s.Sum);
            result.MinesWinSum = minesGames.Where(d => d.Win).Sum(s => s.CanWin - s.Sum);

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
                    GameDate = game.GameTime.AddHours(3).ToString("G"),
                    GameType = GameType.DiceGame,
                    Win = game.Win
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

        [HttpPost("getMultyAccauntsByUserId")]
        public async Task<PaginatedList<UserMultyAccaunt>> GetMultyAccaunts(GetMultyAccauntsByUserIdRequest request)
        {
            return await _userService.GetMultyAccauntsByUserId(request);
        }

        #endregion users

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

        #endregion promocodes

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

        #endregion withdrawals

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

        #endregion payments

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

        #endregion cooperation

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

        [HttpPost("getTopRefferals")]
        public async Task<PaginatedList<UserRefferalInfo>> GetTopRefferals(PaginationRequest request)
        {
            return await _userService.GetUserUserRefferalInfoByPagination(request);
        }

        #endregion top dwGetUserPaymentWithdrawalInfoByPagination

        #region

        [HttpPost("getSettings")]
        public async Task<Settings> GetSettings()
        {
            var res = await _cacheService.ReadCache(CacheConstraints.SETTINGS_KEY);
            return SerializationHelper.Deserialize<Settings>(res);
        }

        [HttpPost("updateSettings")]
        public async Task UpdateSettings(Settings settings)
        {
            var cache = await _cacheService.ReadCache(CacheConstraints.SETTINGS_KEY);

            if (cache != null)
            {
                await _cacheService.DeleteCache(CacheConstraints.SETTINGS_KEY);
            }

            var res = SerializationHelper.Serialize<Settings>(settings);

            await _cacheService.WriteCache(CacheConstraints.SETTINGS_KEY, res, TimeSpan.FromDays(365));
        }

        #endregion

        [HttpPost("startFakeOnline")]
        public async Task StartFakeOnline()
        {
            await Task.Run(async () =>
            {
                Random _random = new Random();

                const int minDelayMilliseconds = 2000; // Минимальная задержка между отправками (2 секунды)
                const int maxDelayMilliseconds = 8000; // Максимальная задержка между отправками (5 секунд)
                int minUserCount = 43; // Минимальное число пользователей
                int maxUserCount = 77; // Максимальное число пользователей
                const int maxDifference = 2; // Максимальная разница между числами
                FakeActiveHelper.FakeUserCount = 61;

                while (true)
                {
                    if (DateTime.Now.Hour > 2 && DateTime.Now.Hour < 8)
                    {
                        minUserCount = 11;
                        maxUserCount = 20;
                    }
                    else if (DateTime.Now.Hour > 8 && DateTime.Now.Hour < 15)
                    {
                        minUserCount = 25;
                        maxUserCount = 55;
                    }
                    else
                    {
                        minUserCount = 43;
                        maxUserCount = 77;
                    }

                    if (FakeActiveHelper.FakeUserCount >= maxUserCount)
                    {
                        FakeActiveHelper.FakeUserCount -= 1;
                    }
                    if (FakeActiveHelper.FakeUserCount <= minUserCount)
                    {
                        FakeActiveHelper.FakeUserCount += 1;
                    }

                    int disconnectedUsers = _random.Next(-1, 0);
                    FakeActiveHelper.FakeUserCount += disconnectedUsers;
                    await _onlineContext.Clients.All.SendAsync("UserDisconnected", FakeActiveHelper.FakeUserCount + FakeActiveHelper.UserCount);

                    int delayMilliseconds = _random.Next(minDelayMilliseconds, maxDelayMilliseconds + 1);
                    await Task.Delay(delayMilliseconds);

                    int connectedUsers = _random.Next(0, maxDifference);
                    FakeActiveHelper.FakeUserCount += connectedUsers;

                    await _onlineContext.Clients.All.SendAsync("UserConnected", FakeActiveHelper.FakeUserCount + FakeActiveHelper.UserCount);

                    delayMilliseconds = _random.Next(minDelayMilliseconds, maxDelayMilliseconds + 1);
                    await Task.Delay(delayMilliseconds);
                }
            }
            );
        }

        [HttpPost("startFakeGames")]
        public async Task StartFakeGames()
        {
            await Task.Run(async () =>
            {
                Random _random = new Random();

                while (true)
                {
                    var apiModel = FakeActiveHelper.GetGameApiModel();
                    var gameJson = JsonConvert.SerializeObject(apiModel);

                    if (DateTime.Now.Hour > 2 && DateTime.Now.Hour < 8)
                    {
                        await Task.Delay(new Random().Next(1000, 3000));

                    }
                    else if (DateTime.Now.Hour > 8 && DateTime.Now.Hour < 15)
                    {
                        await Task.Delay(new Random().Next(500, 1500));
                    }
                    else
                    {
                        await Task.Delay(new Random().Next(100, 500));
                    }

                    await _newGameContext.Clients.All.SendAsync("ReceiveMessage", gameJson);
                }
            }
           );
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
                result += (await _withdrawalsService.GetAllActiveByUserId(referal.Id))
                    .Where(p => p.Status == WithdrawalStatus.New).Sum(s => s.Amount);
            }

            return result;
        }
    }
}