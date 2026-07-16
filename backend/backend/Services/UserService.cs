using backend.DTOs.Auth;
using backend.DTOs.User;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<UserResponseDto> GetById(int id, int currentUserId, bool isAdmin)
        {
            if (id != currentUserId && !isAdmin)
            {
                throw new UnauthorizedAccessException();
            }
            var user = await _repo.GetById(id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            return MapToDto(user);
        }

        public async Task<UserResponseDto> UpdateProfile(int id, UpdateProfileDto dto, int currentUserId, bool isAdmin)
        {
            if (id != currentUserId && !isAdmin)
            {
                throw new UnauthorizedAccessException();
            }
            var user = await _repo.GetById(id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            user.FullName = dto.FullName;
            user.Phone = dto.Phone;
            user.Address = dto.Address;
            user.Avatar = dto.Avatar;
            await _repo.Update(user);
            return MapToDto(user);
        }

        public async Task<List<UserResponseDto>> GetAll()
        {
            var users = await _repo.GetAll();
            return users.Select(MapToDto).ToList();
        }

        public async Task Delete(int id)
        {
            var user = await _repo.GetById(id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");  
            }
            if (!user.IsActive)
            {
                throw new InvalidOperationException("User is not active.");
            }
            user.IsActive = false;
            await _repo.Update(user);
        }

        private static UserResponseDto MapToDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Avatar = user.Avatar,
                Roles = user.UserRoles
                    .Where(x => x.Role != null)
                    .Select(x => x.Role!.Name)
                    .ToList()
            };
        }
    }
}