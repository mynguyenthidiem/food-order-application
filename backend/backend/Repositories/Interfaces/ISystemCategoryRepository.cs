using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface ISystemCategoryRepository
    {
        Task<IEnumerable<SystemCategory>> GetAllAsync();

        Task<SystemCategory?> GetByIdAsync(int id);

        Task<SystemCategory> CreateAsync(SystemCategory systemCategory);

        Task UpdateAsync(SystemCategory systemCategory);

        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    }
}
