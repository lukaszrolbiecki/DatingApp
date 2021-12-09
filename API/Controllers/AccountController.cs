using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using API.Data;
using API.Dto;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;

        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            StringValues x = new StringValues();
            Request.Headers.TryGetValue("LongRequest", out x); //LR: to test long requests
            bool isLong = x.ToString() == "true" ? true : false;

            if (await UserExists(registerDto.Username))
            {
                return BadRequest("Username is taken.");
            }

            using HMACSHA512 hamc = new HMACSHA512();
            var appUser = new AppUser
            {
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hamc.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hamc.Key
            };

            _context.Users.Add(appUser);
            await _context.SaveChangesAsync();
            
            return new UserDto
            {
                Username = appUser.UserName,
                Token = _tokenService.CreateToken(appUser)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto login)
        {

            var appUser = await _context.Users.SingleOrDefaultAsync(x => x.UserName == login.Username);

            if (appUser == null)
            {
                return Unauthorized("Invalid username");
            }

            using HMACSHA512 hmac = new HMACSHA512(appUser.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != appUser.PasswordHash[i])
                {
                    return Unauthorized("Wrong password");
                }
            }

            return new UserDto
            {
                Username = appUser.UserName,
                Token = _tokenService.CreateToken(appUser)
            };

        }

        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

    }
}