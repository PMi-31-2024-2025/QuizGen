using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizGen.BLL.Models.Auth;
using QuizGen.BLL.Services.Interfaces;

namespace QuizGen.API.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("Invalid user credentials");
            
            var result = await _authService.GetCurrentUserAsync(userId);
            if (!result.Success)
                return BadRequest(result.Message);
                
            return Ok(result.Data);
        }

        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("Invalid user credentials");
                
            var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { message = "Password changed successfully" });
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("Invalid user credentials");
                
            var result = await _authService.UpdateProfileAsync(userId, request.Name, request.OpenAiApiKey, request.GptModel);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { message = "Profile updated successfully" });
        }
    }
}
