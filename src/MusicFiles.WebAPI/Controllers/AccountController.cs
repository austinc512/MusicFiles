using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using MusicFiles.Core.Domain.IdentityEntities;
using MusicFiles.Core.DTOs.Request;
using MusicFiles.Core.DTOs.Response;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtService _jwtService;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager, IJwtService jwtService)
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
            if (registerDto.UserType != UserTypeOptions.Customer && registerDto.UserType != UserTypeOptions.Publisher)
            {
                return ErrorResponse("Valid user types are: Customer, Publisher");
            }

            if (await _userManager.FindByEmailAsync(registerDto.Email!) != null)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                return ErrorResponse("A user with this email already exists.");
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
                return ErrorResponse("User registration failed", result.Errors);
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
            // usernames cannot contain the "@" symbol, so this allows us to differentiate our authentication flow(s)
            var user = loginDto.UserNameOrEmail.Contains("@")
                ? await _userManager.FindByEmailAsync(loginDto.UserNameOrEmail)
                : await _userManager.FindByNameAsync(loginDto.UserNameOrEmail);

            if (user is null)
            {
                await _userManager.AccessFailedAsync(new ApplicationUser()); // Fake check to prevent enumeration
                await Task.Delay(TimeSpan.FromSeconds(2));
                return Unauthorized(new { Message = "Invalid username, email, or password." });
            }

            var result =
                await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: false);
            // I need to create an email verification system prior to setting lockoutOnFailure: true
            if (!result.Succeeded)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                return Unauthorized(new { Message = "Invalid username, email, or password." });
            }

            var userRoles = new List<string>(await _userManager.GetRolesAsync(user));
            var authenticationResponse = _jwtService.CreateJwtToken(user, userRoles);

            if (!await UpdateRefreshTokenAsync(user, authenticationResponse))
            {
                // haven't instantiated logger class yet
                // _logger.LogError("Failed to update refresh token for user: {user.Id}", user.Id);
                Console.WriteLine($"Failed to update refresh token for user: {user.Id}", user.Id);
                return Unauthorized(new { Message = "Invalid username, email, or password." });
            }

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
            return Ok(new { IsRegistered = user != null });
        }

        [HttpPost]
        public async Task<IActionResult> IsUsernameRegistered([FromQuery] string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            return Ok(new { IsRegistered = user != null });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateNewAccessToken([FromBody]TokenModel tokenModel)
        {
            if (string.IsNullOrWhiteSpace(tokenModel.Token) || string.IsNullOrWhiteSpace(tokenModel.RefreshToken))
            {
                return BadRequest("Invalid tokens.");
            }

            var principal = _jwtService.GetPrincipalFromJwtToken(tokenModel.Token);
            if (principal == null)
            {
                return BadRequest("Invalid tokens.");
            }

            string? userPublicId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userPublicId) || !Guid.TryParse(userPublicId, out var parsedUserPublicId))
            {
                return BadRequest("Invalid PublicUserId format.");
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PublicUserId == parsedUserPublicId);

            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            if (user.RefreshToken != tokenModel.RefreshToken)
            {
                return Unauthorized("Invalid refresh tokens.");
            }

            if (user.RefreshTokenExpiration <= DateTimeOffset.UtcNow)
            {
                return Unauthorized("Refresh token expired. Please log in again.");
            }

            // if here, we can generate a new JWT token
            var userRoles = new List<string>(await _userManager.GetRolesAsync(user));
            var authenticationResponse = _jwtService.CreateJwtToken(user, userRoles);

            user.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpiration = authenticationResponse.RefreshTokenExpiration;
            await _userManager.UpdateAsync(user);

            return Ok(authenticationResponse);
        }

        private async Task<bool> UpdateRefreshTokenAsync(ApplicationUser user, AuthenticationResponse authResponse)
        {
            user.RefreshToken = authResponse.RefreshToken;
            user.RefreshTokenExpiration = authResponse.RefreshTokenExpiration;

            var updateResult = await _userManager.UpdateAsync(user);
            return updateResult.Succeeded;
        }

        private IActionResult ErrorResponse(string message, object? details = null)
        {
            return BadRequest(new { success = false, message, errors = details });
        }
    }
}