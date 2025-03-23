using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Data.Entities;
using WebAPI.Models.Auth;
using WebAPI.Models;
using WebAPI.Models.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;


namespace WebAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration; // Додано для отримання JWT ключа

        public AuthController(ILogger<AuthController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration) //  
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration; //
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new ApplicationUser { UserName = model.Username, Email = model.Email, FullName = model.FullName };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User registered: {Email}", model.Email); // Лог при успішній реєстрації
                return Ok("User registered successfully");
            }

            if (result.Errors.Any()) // додаєм розширене логування для перевірки помилки при створенні користувача
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning(" Registration failed for {Email}: {Errors}", model.Email, errors);
            }
            else
            {
                _logger.LogWarning("Registration failed for {Email}, but no specific errors were provided.", model.Email);
            }
            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            _logger.LogInformation("Login attempt: {Email}", model.Email); // Лог спроби входу

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                _logger.LogWarning("User not found: {Email}", model.Email); //  Лог відсутності користувача
                return Unauthorized("Invalid credentials.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false); // Більш точна перевірка
            if (!result.Succeeded)
            {
                _logger.LogWarning("Invalid credentials for: {Email}", model.Email); // Лог невдалого входу
                return Unauthorized("Invalid credentials.");
            }

            _logger.LogInformation(" Login successful: {Email}", model.Email); // Лог успішного входу

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKey@123"));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"])); // Використовуємо ключ із конфігурації

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>

            {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //додає роль до існуючих ролей, можливі кілька ролей для одного юзера
        //для перевірки - авторизуватись на свагері через логін і отриманим при логіні токеном на кнопці авторизації
        [HttpPost("assign-role")]    // призначення/додавання ролі  юзера після реєстрації 
        public async Task<IActionResult> AssignRole([FromBody] RoleAssignModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("AssignRole: User not found: {Email}", model.Email); // Лог помилки
                return NotFound("User not found");
            }

            var result = await _userManager.AddToRoleAsync(user, model.Role);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to assign role {Role} to {Email}: {Errors}", model.Role, model.Email, string.Join(", ", result.Errors.Select(e => e.Description))); // Детальний лог помилки
                return BadRequest(result.Errors);
            }

            _logger.LogInformation("Role {Role} assigned to {Email}", model.Role, model.Email); //  Успішне призначення
            return Ok("Role assigned successfully");
        }

        //стирає існуючі ролі і додає нову роль
        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeUserRole([FromBody] RoleAssignModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("ChangeRole: User not found: {Email}", model.Email); // Лог
                return NotFound("User not found");
            }

            // Отримуємо всі ролі користувача
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Видаляємо всі ролі
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                _logger.LogError("Failed to remove roles from {Email}: {Errors}", model.Email, string.Join(", ", removeResult.Errors.Select(e => e.Description))); // 
                return BadRequest("Failed to remove old roles");
            }

            // Додаємо нову роль
            var addResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!addResult.Succeeded)
            {
                _logger.LogError("Failed to add role {Role} to {Email}: {Errors}", model.Role, model.Email, string.Join(", ", addResult.Errors.Select(e => e.Description))); // 
                return BadRequest("Failed to assign new role");
            }

            _logger.LogInformation("Role changed for {Email} to {Role}", model.Email, model.Role); // 
            return Ok($"User {user.Email} now has role {model.Role}");
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // або JwtRegisteredClaimNames.Sub
            var email = User.FindFirstValue(ClaimTypes.Email);           // або JwtRegisteredClaimNames.Email

            _logger.LogInformation("User authenticated via JWT: {Email}, ID: {UserId}", email, userId); // Додали лог

            return Ok(new
            {
                UserId = userId,
                Email = email
            });
        }
    }
}
