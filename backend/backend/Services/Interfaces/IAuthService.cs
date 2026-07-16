using backend.DTOs.Auth;

namespace backend.Services.Interfaces
{
    public interface IAuthService
    {
Task<AuthResponseDto> Register(RegisterDto dto);

        Task<AuthResponseDto?> Login(LoginDto dto);

        Task<UserResponseDto?> GetProfile(int userId);

        Task<bool> ChangePassword(int userId,ChangePasswordDto dto);
    }
}