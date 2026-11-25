using OrderApi.Models;

namespace OrderApi.DTOs
{
    public record PaymentDto(
        int Id,
        int OrderId,
        decimal Amount,
        PaymentStatus PaymentStatus,
        DateTime? PaidAt);

    public record CreatePaymentDto(
        int OrderId,
        decimal Amount,
        PaymentStatus PaymentStatus);
}
