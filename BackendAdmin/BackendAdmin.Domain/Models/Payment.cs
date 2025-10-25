using BackendAdmin.Domain.Enums;

namespace BackendAdmin.Domain.Models;

public class Payment : Entity<Guid>
{
    public Guid ApplicationId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethod Method { get; set; }
    public string Reference { get; set; } = default!;
    public DateTime DatePayment { get; set; }
}
