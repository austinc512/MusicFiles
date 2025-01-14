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
        private readonly SignInManager<ApplicationUser> _signInManager;
        
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
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

            var user = model.UserNameOrEmail.Contains("@")
                ? await _userManager.FindByEmailAsync(model.UserNameOrEmail)
                : await _userManager.FindByNameAsync(model.UserNameOrEmail);

            if (user is null)
            {
                return Unauthorized(new { Message = "Invalid username or email." });
            }

            var result = await _signInManager.PasswordSignInAsync(user, 
                model.UserPassword, isPersistent: model.UserRememberMe, lockoutOnFailure: false);
          
            if (!result.Succeeded) return Unauthorized(new { Message = "Invalid password." });
        
            // _jwt service not implemented yet, but might look something like this:
            // var authenticationResponse = _jwtService.CreateJwtToken(user);
            
            // Update refresh token in user record
            // user.RefreshToken = authenticationResponse.RefreshToken;
            // user.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;
            // await _userManager.UpdateAsync(user);
            
            // return Ok(authenticationResponse);
            
            // placeholder value for the time being
            return Ok(new { Message = "User successfully logged in" });
            
        }
        
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return NoContent();
        }
        
        // Methods for checking whether Email/UserName is already registered
        // Feature: there NEEDS to be rate limiting for these endpoints
        [HttpPost]
        public async Task<IActionResult> IsEmailRegistered([FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return Ok(new
            {
                IsRegistered = user != null
            });
        }

        [HttpPost]
        public async Task<IActionResult> IsUsernameRegistered([FromQuery] string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            return Ok(new
            {
                IsRegistered = user != null
            });
        }
    }

}