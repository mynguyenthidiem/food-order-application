using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<User> Items, int TotalCount)> GetAll(int pageNumber, int pageSize)
        {
            var query = _context.Users
                .Where(u => u.IsActive)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsNoTracking();
            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
            return (items, totalCount);
        }

        public async Task<User?> GetById(int id)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }
        public async Task<User?> GetByEmail(string email)
        {
            var normalizedEmail = email.Trim().ToLower();

            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.IsActive);
        }

        public async Task<bool> ExistsEmail(string email)
        {
            var normalizedEmail = email.Trim().ToLower();

            return await _context.Users
                .AnyAsync(u => u.Email == normalizedEmail && u.IsActive);
        }

        public async Task<User> Create(User user)
        {
            user.Email = user.Email.Trim().ToLower();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task Update(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                user.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Role?> GetRoleByName(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task CreateUserRole(UserRole userRole)
        {
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
        }
    }
}