using Hospitium.AuthService.DTOs;
using Hospitium.AuthService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hospitium.AuthService.Controllers
{
    /// <summary>
    /// Controller for handling authentication-related requests such as login, registration, and password resets.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service.</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            await _authService.RequestPasswordResetAsync(dto);
            return Ok(new { message = "If that email is registered, you will receive an OTP shortly." });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            var isValid = await _authService.VerifyOtpAsync(dto);
            if (!isValid)
                return BadRequest(new { message = "Invalid or expired OTP." });
            return Ok(new { message = "OTP verified successfully." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            await _authService.ResetPasswordAsync(dto);
            return Ok(new { message = "Password reset successfully. You can now log in." });
        }
    }
}
