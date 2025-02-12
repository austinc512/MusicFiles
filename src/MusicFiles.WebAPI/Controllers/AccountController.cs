using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicFiles.Core.Domain.IdentityEntities;
using MusicFiles.Core.DTOs.Request;
using MusicFiles.Core.Enums;
using MusicFiles.Core.ServiceContracts;
using MusicFiles.Core.Services;

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
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtService _jwtService;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            
            // check valid UserTypeOptions before creating ApplicationUser
            // public controller does not create Admin users.
            if (registerDto.UserType != UserTypeOptions.Customer
                && registerDto.UserType != UserTypeOptions.Publisher)
            {
                // fix this error handling later
                // I need a more consistent API for handling errors.
                return BadRequest(new { message = "Valid user types are: Customer, Publisher" });
            }
            
            var user = new ApplicationUser() 
            {
                // custom properties of ApplicationUser
                // PublicUserId set by ApplicationUser constructor
                FirstName = registerDto.FirstName!,
                LastName = registerDto.LastName!,
                // inherited from IdentityUser
                // Id set by IdentityUser constructor
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password!);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            
            // Add User to Role
            await _userManager.AddToRoleAsync(user, registerDto.UserType.ToString());
            
            // I haven't decided whether a user should be able to log in immediately,
            // or if email verification is a required step before doing anything.
            // It's definitely a better user experience to just allow users to create
            // an account immediately, but that has security implications.
            
            return Ok(new { Message = "User registered successfully" });

        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // to clarify, username cannot contain "@" symbol
            var user = loginDto.UserNameOrEmail.Contains("@")
                ? await _userManager.FindByEmailAsync(loginDto.UserNameOrEmail)
                : await _userManager.FindByNameAsync(loginDto.UserNameOrEmail);

            if (user is null)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                return Unauthorized(new { Message = "Invalid username, email, or password." });
            }
            
            // email verification is a separate step that needs to be implemented on Register action first.
            // if (!await _userManager.IsEmailConfirmedAsync(user))
            // {
            //     return Unauthorized(new { Message = "Please confirm your email address before logging in." });
            // }

            var result = await _signInManager.PasswordSignInAsync(user, 
                loginDto.Password, isPersistent: loginDto.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                return Unauthorized(new { Message = "Invalid username, email, or password." });
            }

            var userRoles = new List<string>(await _userManager.GetRolesAsync(user));
            var authenticationResponse = _jwtService.CreateJwtToken(user, userRoles);
            
            // Update refresh token in user record
            // user.RefreshToken = authenticationResponse.RefreshToken;
            // user.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;
            // await _userManager.UpdateAsync(user);
            
            return Ok(authenticationResponse);
            
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