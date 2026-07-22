using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);

        Task<Payment?> GetByOrderIdAsync(int orderId);

        Task<Payment?> GetPaymentWithOrderAsync(int orderId);
        Task<Payment?> GetByIdAsync(int id);

        Task SaveChangesAsync();
    }
}