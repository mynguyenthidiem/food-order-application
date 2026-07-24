using backend.DTOs.Page;
using backend.DTOs.Notification;
namespace backend.Services.Interfaces
{
    public interface INotificationService
    { 
        Task Create(CreateNotificationRequest request);

        Task<PagedResultDto<NotificationDto>> GetByUser(int userId, PaginationParams pagination);

        Task<int> GetUnreadCount(int userId);

        Task MarkAsRead(int notificationId, int userId);

        Task MarkAllAsRead(int userId);
    }
}
