using Hospitium.AuthService.DTOs;

namespace Hospitium.AuthService.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for authentication-related services.
    /// </summary>
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task RequestPasswordResetAsync(ForgotPasswordDto dto);
        Task<bool> VerifyOtpAsync(VerifyOtpDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        Task<IEnumerable<UserDto>> GetAllManagersAsync();
    }
}
