using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicFiles.Core.DTOs.Request;

namespace MusicFiles.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        // enforcing private readonly fields doesn't work cleanly with primary constructors
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        
        public AccountController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            // if (!ModelState.IsValid) return BadRequest(ModelState); // redundant? 
            Console.WriteLine(ModelState.ErrorCount);

            var user = new IdentityUser 
            {
                UserName = model.Email, 
                Email = model.Email,
                PhoneNumber = model.Phone
                
            };
            var result = await _userManager.CreateAsync(user, model.Password!);

            if (result.Succeeded)
            {
                return Ok(new { Message = "User registered successfully" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _signInManager.PasswordSignInAsync(model.UserEmail, model.UserPassword, model.UserRememberMe = false, false);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Login successful" });
            }

            return Unauthorized(new { Message = "Invalid email or password" });
        }
        
        // Methods for checking whether Email/UserName is already registered
        // False is good here, you dummy.
        // Feature: there NEEDS to be rate limiting for these endpoints
        [HttpPost]
        public async Task<IActionResult> IsEmailRegistered(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return Ok(new
            {
                IsRegistered = user != null
            });
        }

        [HttpPost]
        public async Task<IActionResult> IsUsernameRegistered(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            return Ok(new
            {
                IsRegistered = user != null
            });
        }
    }

}

