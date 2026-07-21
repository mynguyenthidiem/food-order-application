using backend.Models;

namespace backend.Services.Interfaces
{
    public interface IPaymentService
    {
        Task CreatePayment(int orderId,decimal amount,PaymentMethod method);
        Task<Payment?> GetByOrderId(int orderId);
        Task CompletePayment(int orderId, int ownerId, string? transactionId);
        Task FailPayment(int orderId);
    }
}