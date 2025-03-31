using DrugIndication.Application.Services;
using DrugIndication.Domain.DTO;
using DrugIndication.Domain.Entities;
using DrugIndication.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DrugIndication.API.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _userRepo;
        private readonly AuthService _auth;

        public AuthController(UserRepository userRepo, AuthService auth)
        {
            _userRepo = userRepo;
            _auth = auth;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(AuthRegisterDto dto)
        {
            var existing = await _userRepo.GetByUsernameAsync(dto.Username);
            if (existing != null)
                return BadRequest("Username already exists");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
            };

            await _userRepo.CreateAsync(user);
            return Ok("User created");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthLoginDto dto)
        {
            var user = await _userRepo.GetByUsernameAsync(dto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var token = _auth.GenerateToken(user);
            return Ok(new { token });
        }
    }
}
