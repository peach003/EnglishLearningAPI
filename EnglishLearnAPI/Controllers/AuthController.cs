using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnglishLearningAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace EnglishLearningAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // user registration interface
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new { error = "Email and Password are required." });
            }

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return BadRequest(new { error = "Email is already in use." });

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = string.IsNullOrEmpty(model.FullName) ? "Anonymous User" : model.FullName, 
                AvatarUrl = "/assets/avatar.png" 
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "User registered successfully!" });
        }

        // User Login Interface
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new { error = "Email and Password are required." });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null) return Unauthorized(new { error = "Invalid credentials" });
            
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded) return Unauthorized(new { error = "Invalid credentials" });

            var token = GenerateJwtToken(user);
            return Ok(new 
            { 
                token,
                fullName = user.FullName,  
                email = user.Email,
                avatarUrl = user.AvatarUrl
            });
        }

        //  Access to user information
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {


            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized(new { error = "Invalid token" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { error = "User not found" });

            return Ok(new
            {
                fullName = user.FullName,
                email = user.Email,
                avatarUrl = user.AvatarUrl
            });
        }
        public string GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id), 
                new Claim(ClaimTypes.Name, user.UserName), // 用户名
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
                new Claim("FullName", user.FullName ?? "Anonymous User"),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            };
    
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"], 
                audience: jwtSettings["Audience"], 
                claims: claims,
                expires: DateTime.Now.AddHours(2), 
                signingCredentials: creds 
            );
    
            return new JwtSecurityTokenHandler().WriteToken(token); 
        }

        private string GenerateJwtToken1(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key         = jwtSettings["Key"];
            var issuer      = jwtSettings["Issuer"];
            var audience    = jwtSettings["Audience"];

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                throw new InvalidOperationException("JWT configuration is missing.");
            }

            // 确保用户ID有效
            if (string.IsNullOrEmpty(user.Id))
            {
                throw new ArgumentException("User ID cannot be null or empty.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id), 
                new Claim(JwtRegisteredClaimNames.Sub, user.Id), 
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim("FullName", user.FullName ?? "Anonymous User"),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"])), 
                    SecurityAlgorithms.HmacSha256
                )
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
            
        }
    }
}
