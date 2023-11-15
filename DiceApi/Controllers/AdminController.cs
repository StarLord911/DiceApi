using AutoMapper;
using DiceApi.Attributes;
using DiceApi.Data;
using DiceApi.Data.Api.Model;
using DiceApi.Data.Data.Admin;
using DiceApi.Data.Data.Payment;
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

        public AdminController(IUserService userService,
            IPaymentService paymentService,
            IWithdrawalsService withdrawalsService,
            IMapper mapper)
        {
            _userService = userService;
            _paymentService = paymentService;
            _withdrawalsService = withdrawalsService;
            _mapper = mapper;
        }

        [Authorize(true)]
        [HttpPost("getUsersByPage")]
        public async Task<List<UserApi>> GetUsersByPage(GetUsersByPaginationRequest request)
        {
            var users = await _userService.GetUsersByPagination(request);

            return users.Select(u => _mapper.Map<UserApi>(u)).ToList();
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



    }
}
