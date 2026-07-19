using backend.DTOs.Auth;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        public async Task<AuthResponseDto> Register(RegisterDto dto)
        {
            if (await _repo.ExistsEmail(dto.Email))
            {
                throw new Exception("Email has already been used.");
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
            var role = await _repo.GetRoleByName("Customer");
            if (role != null)
            {
                await _repo.CreateUserRole(new UserRole
                {
                    UserId = createdUser.Id,
                    RoleId = role.Id
                });
            }
            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful.",
                User = MapUser(createdUser)
            };
        }

        public async Task<AuthResponseDto?> Login(LoginDto dto)
        {
            var user = await _repo.GetByEmail(dto.Email);
            if (user == null)
            {
                throw new Exception("Email or password is incorrect.");
            }
            if (!user.IsActive)
            {
                throw new Exception("Account has been locked.");
            }
            bool passwordCorrect = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);
            if (!passwordCorrect)
            {
                throw new Exception("Email or password is incorrect.");
            }
            var token = GenerateToken(user);
            return new AuthResponseDto
            {
                Success = true,
                Message = "Log in successful.",
                Token = token,
                User = MapUser(user)
            };
        }

        public async Task<UserResponseDto?> GetProfile(int userId)
        {
            var user = await _repo.GetById(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }
            if (!user.IsActive)
            {
                throw new Exception("Your account has been locked.");
            }
            return MapUser(user);
        }

        public async Task<bool> ChangePassword(int userId, ChangePasswordDto dto)
        {
            var user = await _repo.GetById(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }
            bool oldPasswordCorrect = BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.Password);
            if (!oldPasswordCorrect)
            {
                throw new Exception("Current password is incorrect.");
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _repo.Update(user);
            return true;
        }

        private string GenerateToken(User user)
        {
            var claims = new List<Claim>{
                    new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                    new Claim(ClaimTypes.Email,user.Email)
                };
            foreach (var ur in user.UserRoles)
            {
                if (ur.Role != null)
                {
                    claims.Add(
                        new Claim(ClaimTypes.Role, ur.Role.Name));
                }
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            int expiry = int.Parse(_config["Jwt:ExpiryInMinutes"] ?? "60");
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiry),
                signingCredentials: credential
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserResponseDto MapUser(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Avatar = user.Avatar,
                Roles =user.UserRoles
                    .Where(x => x.Role != null)
                    .Select(x => x.Role!.Name)
                    .ToList()
            };
        }
    }
}