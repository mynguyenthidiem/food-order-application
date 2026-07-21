using backend.Models;

public class PaymentResponseDto
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; }

    public string? TransactionId { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CompletePaymentDto
{
    public string? TransactionId { get; set; }
}
