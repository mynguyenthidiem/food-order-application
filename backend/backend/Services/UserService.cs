using backend.DTOs.Auth;
using backend.DTOs.Page;
using backend.DTOs.User;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IFileStorageService _fileStorageService;

        public UserService(IUserRepository repo, IFileStorageService fileStorageService)
        {
            _repo = repo;
            _fileStorageService = fileStorageService;
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

            if (dto.Avatar != null)
            {
                await _fileStorageService.DeleteImage(user.Avatar);

                user.Avatar = await _fileStorageService.SaveImage(dto.Avatar, "Users");
            }

            await _repo.Update(user);
            return MapToDto(user);
        }

        public async Task<PagedResultDto<UserResponseDto>> GetAll(PaginationParams pagination)
        {
            var (items, totalCount) = await _repo.GetAll(pagination.PageNumber, pagination.PageSize);
            return new PagedResultDto<UserResponseDto>(items.Select(MapToDto).ToList(), totalCount, pagination.PageNumber, pagination.PageSize);
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

        public async Task<UserResponseDto> CreateOwner(CreateOwnerDto dto)
        {
            if (await _repo.ExistsEmail(dto.Email))
            {
                throw new InvalidOperationException("Email already exists.");
            }

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone,
                Address = dto.Address,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _repo.Create(user);

            var ownerRole = await _repo.GetRoleByName("Owner");

            if (ownerRole == null)
            {
                throw new KeyNotFoundException("Owner role not found.");
            }

            await _repo.CreateUserRole(new UserRole
            {
                UserId = createdUser.Id,
                RoleId = ownerRole.Id
            });

            var result = await _repo.GetById(createdUser.Id);

            return MapToDto(result!);
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