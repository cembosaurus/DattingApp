using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Custom;
using AutoMapper;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private IMapper _mapper;

        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _repo = repo;
            _config = config;
            _mapper = mapper;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userToRegisterDto)
        {
            userToRegisterDto.UserName = userToRegisterDto.UserName.ToLower();

            if(await _repo.UserExist(userToRegisterDto.UserName))
                return BadRequest("User already exist in dabtabase!");

            User createdUser = _mapper.Map<User>(userToRegisterDto);

            var registeredUser = await _repo.Register(createdUser, userToRegisterDto.Password);

            var userToReturn = _mapper.Map<UserForDetailedDto>(registeredUser);

            return CreatedAtRoute("GetUser", new { controller = "Users", id = createdUser.Id, }, userToReturn);

        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto user)
        {

            //throw new Exception("... this shit is getting on my nerves - exception ! ....");

            var userFromRepo = await _repo.Login(user.UserName.ToLower(), user.Password);

            if(userFromRepo == null)
                return Unauthorized();

            var secret = _config.GetSection("Cembo_Settings:Token").Value;

            var token = new JWTToken(userFromRepo, secret);

            var tokenBuild = token.Build();

            var userFromApi = _mapper.Map<UserForListDto>(userFromRepo);

            return Ok(new {
                token = tokenBuild,
                userFromApi
            });

        }

    }
}