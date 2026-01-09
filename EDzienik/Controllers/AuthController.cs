using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EDzienik.DTOs.Auth;
using EDzienik.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using NuGet.Protocol.Plugins;

namespace EDzienik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;

        }


        //POST /api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) 
                return Unauthorized(new { message = "Invalid email or password" });

            var SignInResult = await _signInManager.PasswordSignInAsync(
                userName: user.UserName!,
                password: dto.Password,
                isPersistent: false,
                lockoutOnFailure: false
            );

            if (!SignInResult.Succeeded) 
                return Unauthorized(new {message = "Invalid email or password"});

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "None";

            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                role
            });
        }

        //POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors);

            var roleName = dto.Role.ToString();

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleCreateResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleCreateResult.Succeeded)
                    return BadRequest(roleCreateResult.Errors);
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!addRoleResult.Succeeded)
                return BadRequest(addRoleResult.Errors);

            return StatusCode(201, new
            {
                userId = user.Id,
                email = user.Email,
                role = roleName
            });
        }


    }
}
