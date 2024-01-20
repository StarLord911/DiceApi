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
    [EnableCors("AllowAll")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ICacheService _cacheService;
        private readonly IWageringRepository _wageringRepository;


        public UserController(IUserService userService,
            ITelegramBotClient telegramBotClient,
            ICacheService cacheService,
            IWageringRepository wageringRepository,
            IMapper mapper)
        {
            _userService = userService;
            _telegramBotClient = telegramBotClient;
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

        [Authorize]
        [HttpPost("getUserById")]
        public UserApi GetUserById(GetByUserIdRequest request)
        {
            var user = _userService.GetById(request.Id);

            if (user == null)
            {
                throw new NullReferenceException($"Cannot find user by id {user}");
            }

            return _mapper.Map<UserApi>(user);
        }

        [Authorize]
        [HttpPost("getDailyBonusByUserId")]
        public async Task<string> GetDailyBonusByUserId(GetByUserIdRequest request)
        {
            var cache = await _cacheService.ReadCache(CacheConstraints.EVERY_DAY_BONUS + request.Id);

            if (!string.IsNullOrEmpty(cache))
            {
                return "Alredy use bonus";
            }
            var user = _userService.GetById(request.Id);

            if (false) //TODO: привязка тг
            {
                return "Telegram use bonus";
            }

            var bonus = new Random().Next(10);

            var newBallance = user.Ballance + bonus;

            await _cacheService.WriteCache(CacheConstraints.EVERY_DAY_BONUS + request.Id, "true", TimeSpan.FromHours(24));

            var wager = await _wageringRepository.GetActiveWageringByUserId(request.Id);
            await _wageringRepository.UpdateWagering(request.Id, wager.Wagering + (bonus * 20));
            await _userService.UpdateUserBallance(request.Id, newBallance);

            return "Succes";
        }

        [Authorize]
        [HttpPost("changeUserData")]
        public async Task ChangeUserInformation(ChangeUserInformationRequest request)
        {
            var user = _userService.GetById(request.UserId);

            await _userService.UpdateUserInformation(new UpdateUserRequest {UserId = request.UserId, Name = request.NewName, Password = request.NewPassword });
        }


            #region

            //[HttpGet]
            //public IActionResult Login()
            //{
            //    // Перенаправление пользователя на страницу авторизации Telegram
            //    string redirectUrl = $"https://telegram.me/{_telegramBotClient.GetMeAsync().Result.Username}?start=auth";
            //    return Redirect(redirectUrl);
            //}

            //[HttpPost]
            //public IActionResult Auth([FromBody] Update update)
            //{
            //    // Parsed from the query string / from the callback object
            //    Dictionary<string, string> fields = QueryStringFields;

            //    LoginWidget loginWidget = new LoginWidget("your API access Token");
            //    if (loginWidget.CheckAuthorization(fields) == Authorization.Valid)
            //    {
            //        // ...
            //    }
            //}

            #endregion
        }
}