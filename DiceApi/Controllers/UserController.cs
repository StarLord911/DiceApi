using AutoMapper;
using DiceApi.Attributes;
using DiceApi.Data;
using DiceApi.Data.Api.Model;
using DiceApi.Data.Data.Payment;
using DiceApi.Data.Requests;
using DiceApi.Services;
using DiceApi.Services.Contracts;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiceApi.Controllers
{
    [Route("api/useController")]
    [EnableCors("AllowAll")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService,
            IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(AuthenticateRequest model)
        {
            var response = await _userService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterResponce userModel)
        {
            var response = await _userService.Register(userModel);

            if (response == null)
            {
                return BadRequest(new { message = "Didn't register!" });
            }

            return Ok(response);
        }

        [Authorize]
        [HttpPost("getUserById")]
        public UserApi GetUserById(GetUserByIdRequest request)
        {
            var user = _userService.GetById(request.Id);

            if (user == null)
            {
                throw new NullReferenceException($"Cannot find user by id {user}");
            }

            return _mapper.Map<UserApi>(user);
        }
    }
}

