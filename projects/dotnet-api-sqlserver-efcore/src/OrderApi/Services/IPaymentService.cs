using OrderApi.DTOs;
using OrderApi.Models;

namespace OrderApi.Services
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentDto>> GetPaymentsAsync();
        Task<PaymentDto?> GetPaymentByIdAsync(int id);
        Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto);
        Task<PaymentDto?> UpdatePaymentStatusAsync(int id, PaymentStatus status);
        Task<bool> DeletePaymentAsync(int id);
    }
}
