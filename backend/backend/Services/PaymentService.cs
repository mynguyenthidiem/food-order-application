using backend.Data;
using backend.Models;
using backend.Repositories;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;

        public PaymentService(IPaymentRepository repo)
        {
            _repo = repo;
        }

        public async Task CreatePayment(int orderId, decimal amount, PaymentMethod method)
        {
            var payment = new Payment
            {
                OrderId = orderId,
                Amount = amount,
                Method = method,
                Status = PaymentStatus.Pending
            };

            await _repo.AddAsync(payment);
            await _repo.SaveChangesAsync();
        }

        public async Task<Payment?> GetByOrderId(int orderId)
        {
            return await _repo.GetByOrderIdAsync(orderId);
        }

        public async Task CompletePayment(int orderId, int ownerId, string? transactionId)
        {
            var payment = await _repo.GetPaymentWithOrderAsync(orderId);

            if (payment == null)
            {
                throw new KeyNotFoundException("Payment not found.");
            }

            if (payment.Order == null)
            {
                throw new InvalidOperationException("Order associated with this payment was not found.");
            }

            if (payment.Order.Restaurant == null)
            {
                throw new InvalidOperationException("Restaurant information for this order is missing.");
            }

            if (payment.Order.Restaurant.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException("You are not authorized to complete payment for this restaurant's order.");
            }

            if (payment.Method != PaymentMethod.COD)
            {
                throw new InvalidOperationException("Only COD payments can be completed manually.");
            }

            if (payment.Status == PaymentStatus.Completed)
            {
                throw new InvalidOperationException("Payment has already been completed.");
            }

            payment.Status = PaymentStatus.Completed;
            payment.TransactionId = transactionId;

            await _repo.SaveChangesAsync();
        }

        public async Task FailPayment(int orderId)
        {
            var payment = await GetByOrderId(orderId);
            if (payment == null)
            {
                throw new KeyNotFoundException("Payment not found.");
            }

            if (payment.Status == PaymentStatus.Completed)
            {
                throw new InvalidOperationException("Completed payment cannot be marked as failed.");
            }

            payment.Status = PaymentStatus.Failed;
            await _repo.SaveChangesAsync();
        }
    }
}