using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinecraftBackend.Data;
using MinecraftBackend.Models;
using MinecraftBackend.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace MinecraftBackend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { Message = "All fields are required." });
            }

            
            if (request.Password.Length < 6)
            {
                return BadRequest(new { Message = "Password must be at least 6 characters long." });
            }

            
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(request.Email))
            {
                return BadRequest(new { Message = "Invalid email format." });
            }

            
            if (await _context.Users.AnyAsync(u => u.Email == request.Email)) return BadRequest(new { Message = "Email already exists!" });
            if (await _context.Users.AnyAsync(u => u.Username == request.Username)) return BadRequest(new { Message = "Username already taken!" });

            
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "User",
                Status = "Active",
                CreatedAt = DateTime.Now
            };
            _context.Users.Add(user);

            
            var defaultProfile = new PlayerProfile
            {
                CharacterID = Guid.NewGuid().ToString(),
                UserId = user.Id,
                DisplayName = request.Username,
                Level = 1,
                Exp = 0,
                Gold = 1000,
                Gem = 10,
                AvatarUrl = "/images/avatars/steve.png",
                GameMode = "Survival",
                Health = 100,
                MaxHealth = 100,
                Hunger = 100
            };
            _context.PlayerProfiles.Add(defaultProfile);

            
            _context.Transactions.Add(new Transaction
            {
                UserId = user.Id,
                ActionType = "REGISTER",
                Details = "Account Created",
                CreatedAt = DateTime.Now,
                Amount = 0,
                CurrencyType = "NONE"
            });

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Registration successful! Please login." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest(new { Message = "Invalid email or password." });
            }

            if (user.Status != "Active") return BadRequest(new { Message = "Account is banned." });

            
            string token = CreateToken(user);

            
            _context.Transactions.Add(new Transaction
            {
                UserId = user.Id,
                ActionType = "LOGIN",
                Details = "User Login",
                CreatedAt = DateTime.Now,
                Amount = 0,
                CurrencyType = "NONE"
            });

            await _context.SaveChangesAsync();

            return Ok(new { Token = token, UserId = user.Id, Username = user.Username });
        }

        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto req)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            if (!BCrypt.Net.BCrypt.Verify(req.OldPassword, user.PasswordHash))
                return BadRequest(new { Message = "Incorrect old password." });

            if (req.NewPassword.Length < 6)
                return BadRequest(new { Message = "New password too short." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Password changed successfully!" });
        }

        [HttpGet("characters")]
        [Authorize]
        public async Task<IActionResult> GetCharacters()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var chars = await _context.PlayerProfiles.Where(p => p.UserId == userId)
                .Select(p => new CharacterDto
                {
                    CharacterID = p.CharacterID,
                    CharacterName = p.DisplayName,
                    Level = p.Level,
                    GameMode = p.GameMode,
                    AvatarUrl = p.AvatarUrl,
                    Gold = p.Gold
                })
                .ToListAsync();
            return Ok(chars);
        }

        [HttpPost("character")]
        [Authorize]
        public async Task<IActionResult> CreateCharacter([FromBody] CreateCharacterDto req)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int count = await _context.PlayerProfiles.CountAsync(p => p.UserId == userId);
            if (count >= 3) return BadRequest(new { Message = "Max 3 characters allowed." });

            var newProfile = new PlayerProfile
            {
                CharacterID = Guid.NewGuid().ToString(),
                UserId = userId,
                DisplayName = req.CharacterName,
                GameMode = req.GameMode ?? "Survival",
                Level = 1,
                Exp = 0,
                Gold = 100,
                Gem = 0,
                Health = 100,
                MaxHealth = 100,
                Hunger = 100,
                AvatarUrl = "/images/avatars/steve.png"
            };
            _context.PlayerProfiles.Add(newProfile);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Character created!", CharacterId = newProfile.CharacterID });
        }

        

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));

            
            
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class RegisterDto { public string Username { get; set; } public string Email { get; set; } public string Password { get; set; } }
    public class LoginDto { public string Email { get; set; } public string Password { get; set; } }
    public class CreateCharacterDto { public string CharacterName { get; set; } public string GameMode { get; set; } }
    public class ChangePasswordDto { public string OldPassword { get; set; } public string NewPassword { get; set; } }
}