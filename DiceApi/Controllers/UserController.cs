using AutoMapper;
using DiceApi.Attributes;
using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.Api.Model;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Requests;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.LoginWidget;
using Telegram.Bot.Types;

namespace DiceApi.Controllers
{
    [Route("api/useController")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly IWageringRepository _wageringRepository;


        public UserController(IUserService userService,
            ICacheService cacheService,
            IWageringRepository wageringRepository,
            IMapper mapper)
        {
            _userService = userService;
            _cacheService = cacheService;
            _wageringRepository = wageringRepository;

            _mapper = mapper;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(AuthenticateRequest model)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            model.AuthIpAddres = ipAddress;
            //TODO: добвить запись айпи входа
            var response = await _userService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        [HttpPost("authenticateAdminPage")]
        public async Task<IActionResult> AuthenticateAdmin(AuthenticateRequest model)
        {
            var response = await _userService.Authenticate(model);
            var user = _userService.GetById(response.Id);

            if (user.Role != "Admin")
            {
                return BadRequest(new { message = "User role is incorrect" });
            }

            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterResponce userModel)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (userModel.Name.Length < 5)
            {
                return BadRequest(new { message = "Short name. Name leght should be > 5" });
            }

            if (userModel.Password.Length < 5 || userModel.Password.Length > 15)
            {
                return BadRequest(new { message = "Short password. Password leght should be > 5 and < 15" });

            }

            //TODO: добвить запись айпи входа
            var userRegister = _mapper.Map<UserRegistrationModel>(userModel);
            userRegister.IpAddres = ipAddress;

            var response = await _userService.Register(userRegister);

            if (response == null)
            {
                return BadRequest(new { message = "Didn't register!" });
            }

            return Ok(response);
        }

        [HttpPost("registerTelegram")]
        public async Task<IActionResult> Register(UserTelegramRegisterResponce userModel)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var userRegister = _mapper.Map<UserRegistrationModel>(userModel);
            userRegister.IpAddres = ipAddress;
            userRegister.TelegramUserId = userModel.TelegramUserId;

            var response = await _userService.Register(userRegister);

            if (response == null)
            {
                return BadRequest(new { message = "Didn't register!" });
            }

            return Ok(response);
        }

        [HttpPost("isTelegramUserRegistred")]
        public async Task<bool> IsTelegramUserRegistred(IsTelegramUserRegistredRequest telegramId)
        {
            return await _userService.IsTelegramUserRegistred(telegramId.TelegramUserId);
        }

        [Authorize]
        [HttpPost("getUserById")]
        public UserApi GetUserById(GetByUserIdRequest request)
        {
            var user = _userService.GetById(request.UserId);

            if (user == null)
            {
                throw new NullReferenceException($"Cannot find user by id {user}");
            }

            return _mapper.Map<UserApi>(user);
        }

        [Authorize]
        [HttpPost("getDailyBonusByUserId")]
        public async Task<GetDailyBonusByUserIdResponce> GetDailyBonusByUserId(GetByUserIdRequest request)
        {
            var cache = await _cacheService.ReadCache<object>(CacheConstraints.EVERY_DAY_BONUS + request.UserId);

            if (cache != null)
            {
                return new GetDailyBonusByUserIdResponce { Message = "Вы уже получили бонус сегодня.", Success = false };
            }

            var user = _userService.GetById(request.UserId);

            if (user.TelegramUserId != null)
            {
                return new GetDailyBonusByUserIdResponce { Message = "Telegram use bonus", Success = false};
            }

            var bonus = new Random().Next(5);

            var newBallance = user.Ballance + bonus;

            await _cacheService.WriteCache(CacheConstraints.EVERY_DAY_BONUS + request.UserId, "true", TimeSpan.FromHours(24));

            var wager = await _wageringRepository.GetActiveWageringByUserId(request.UserId);

            if (wager != null)
            {
                await _wageringRepository.UpdateWagering(request.UserId, wager.Wagering + (bonus * 20));
            }
            else
            {
                var newWager = new Wager
                {
                    Wagering = bonus * 20,
                    IsActive = true,
                    Played = 0,
                    UserId = request.UserId
                };

                await _wageringRepository.AddWearing(newWager);
            }

            await _userService.UpdateUserBallance(request.UserId, newBallance);

            return new GetDailyBonusByUserIdResponce { Message = $"Вы получили ежедневный бонус {bonus}", Success = true };
        }

        [Authorize]
        [HttpPost("changeUserData")]
        public async Task ChangeUserInformation(ChangeUserInformationRequest request)
        {
            await _userService.UpdateUserInformation(new UpdateUserRequest { UserId = request.UserId, Name = request.NewName, Password = request.NewPassword });
        }
    }
}