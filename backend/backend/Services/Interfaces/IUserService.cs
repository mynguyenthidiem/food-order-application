using backend.DTOs.Auth;
using backend.DTOs.User;

namespace backend.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto> GetById(int id, int currentUserId, bool isAdmin);
        Task<UserResponseDto> UpdateProfile(int id, UpdateProfileDto dto, int currentUserId, bool isAdmin);
        Task Delete(int id);
        Task<List<UserResponseDto>> GetAll();
        Task<UserResponseDto> CreateOwner(CreateOwnerDto dto);
    }
}

