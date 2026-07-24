
using backend.DTOs.Notification;
using backend.DTOs.Page;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;

        public NotificationService(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task Create(CreateNotificationRequest request)
        {
            var notification = new Notification
            {
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                RelatedEntityId = request.RelatedEntityId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.Create(notification);
        }

        public async Task<PagedResultDto<NotificationDto>> GetByUser(int userId, PaginationParams pagination)
        {
            var (items, totalCount) = await _repository.GetByUserId(userId, pagination.PageNumber, pagination.PageSize);
            return new PagedResultDto<NotificationDto>(items.Select(MapToDto).ToList(), totalCount, pagination.PageNumber, pagination.PageSize);
        }

        public async Task<int> GetUnreadCount(int userId)
        {
            return await _repository.GetUnreadCount(userId);
        }

        public async Task MarkAsRead(int notificationId, int userId)
        {
            var notification = await _repository.GetById(notificationId);
            if (notification == null)
            {
                throw new KeyNotFoundException("Notification not found.");
            }

            if (notification.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not allowed to access this notification.");
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _repository.SaveChanges();
            }
        }

        public async Task MarkAllAsRead(int userId)
        {
            var unread = await _repository.GetUnreadByUserId(userId);
            if (unread.Count == 0)
            {
                return;
            }

            foreach (var notification in unread)
            {
                notification.IsRead = true;
            }

            await _repository.SaveChanges();
        }

        private static NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                RelatedEntityId = notification.RelatedEntityId,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}
