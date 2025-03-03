using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Data.Entities;
using WebAPI.Models.Auth;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.CheckPasswordAsync(user, model.Password)))
                return Unauthorized("Invalid login or password");

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKey@123"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
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
                return NotFound("User not found");

            var result = await _userManager.AddToRoleAsync(user, model.Role);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Role assigned successfully");
        }

        //стирає існуючі ролі і додає нову роль
        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeUserRole([FromBody] RoleAssignModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return NotFound("User not found");

            // Отримуємо всі ролі користувача
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Видаляємо всі ролі
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return BadRequest("Failed to remove old roles");

            // Додаємо нову роль
            var addResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!addResult.Succeeded) return BadRequest("Failed to assign new role");

            return Ok($"User {user.Email} now has role {model.Role}");
        }


    }
}
