using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> Create(Notification notification);

        Task<Notification?> GetById(int id);

        Task<(List<Notification> Items, int TotalCount)> GetByUserId(int userId, int pageNumber, int pageSize);

        Task<List<Notification>> GetUnreadByUserId(int userId);

        Task<int> GetUnreadCount(int userId);

        Task SaveChanges();
    }
}
