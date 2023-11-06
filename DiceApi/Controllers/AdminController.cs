using AutoMapper;
using DiceApi.Attributes;
using DiceApi.Data;
using DiceApi.Data.Api.Model;
using DiceApi.Services;
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

        public AdminController(IUserService userService,
            IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [Authorize(true)]
        [HttpPost("getUsersByPage")]
        public async Task<List<UserApi>> GetUsersByPage(GetUsersByPaginationRequest request)
        {
            var users = await _userService.GetUsersByPagination(request);

            return users.Select(u => _mapper.Map<UserApi>(u)).ToList();
        }

        
    }
}
