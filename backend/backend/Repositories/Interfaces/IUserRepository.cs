using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAll();
        Task<User?> GetById(int id);
        Task<User?> GetByEmail(string email);
        Task<bool> ExistsEmail(string email);
        Task<Role?> GetRoleByName(string roleName);
        Task CreateUserRole(UserRole userRole);
        Task<User> Create(User user);
        Task Update(User user);
        Task Delete(int id);
    }
}