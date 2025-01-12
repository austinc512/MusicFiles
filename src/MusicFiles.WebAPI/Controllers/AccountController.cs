using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicFiles.Core.Domain.IdentityEntities;
using MusicFiles.Core.DTOs.Request;

namespace MusicFiles.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        // enforcing private readonly fields doesn't work cleanly with primary constructors
        // To-Do: wrap auth into a Service
        // this service can be used by other background services outside of Controllers
        // better adherence to Clean Architecture and Hexagonal Architecture
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationRole> _signInManager;
        
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationRole> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            
            
            var user = new ApplicationUser() 
            {
                // custom properties of ApplicationUser
                // PublicUserId set by ApplicationUser constructor
                FirstName = model.FirstName!,
                LastName = model.LastName!,
                // inherited from IdentityUser
                // Id set by IdentityUser constructor
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
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

            var result = await _signInManager.PasswordSignInAsync(model.UserEmail, model.UserPassword, model.UserRememberMe = false, false);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Login successful" });
            }

            return Unauthorized(new { Message = "Invalid email or password" });
        }
        
        // Methods for checking whether Email/UserName is already registered
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

