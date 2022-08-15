using System;
using RabbitMQ.Client;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;
using TestProjectAPI.Tools;
using DLL.Entities;
using DLL.Services;
using DLL.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace TestProjectAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
  
        private readonly IRabbitMqService _mqService;

        private readonly UsersService _usersService;

        public UserController(IRabbitMqService mqService, UsersService usersService)
        {      
            _mqService = mqService;
            _usersService = usersService;
        }

        [AllowAnonymous]
        [HttpPost("auth")]
        public string Auth(CredentialsDTO credentials)
        {
            var result = _usersService.Authenticate(credentials);
            return result;
        }

        [Authorize]
        [HttpGet]
        public async Task<List<User>> Get() =>
         await _usersService.GetAsync();

        [Authorize]
        [HttpPost]
        public IActionResult Post(string userName)
        {
            try
            {
                _mqService.SendMessage(userName);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

    }
}