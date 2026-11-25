using Microsoft.EntityFrameworkCore;
using OrderApi.DTOs;
using OrderApi.Models;

namespace OrderApi.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsAsync()
        {
            return await _context.Payments
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .Select(p => new PaymentDto(
                    p.Id,
                    p.OrderId,
                    p.Amount,
                    p.PaymentStatus,
                    p.PaidAt))
                .ToListAsync();
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
        {
            var payment = await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return payment == null
                ? null
                : new PaymentDto(payment.Id, payment.OrderId, payment.Amount, payment.PaymentStatus, payment.PaidAt);
        }

        public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto)
        {
            if (dto.Amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dto), "Payment amount must be greater than zero.");
            }

            var order = await _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
            {
                throw new InvalidOperationException("Order not found.");
            }

            if (order.Payment != null)
            {
                throw new InvalidOperationException("This order already has a payment record.");
            }

            var payment = new Payment
            {
                OrderId = dto.OrderId,
                Amount = dto.Amount,
                PaymentStatus = dto.PaymentStatus,
                PaidAt = dto.PaymentStatus == PaymentStatus.Success ? DateTime.UtcNow : null
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return new PaymentDto(payment.Id, payment.OrderId, payment.Amount, payment.PaymentStatus, payment.PaidAt);
        }

        public async Task<PaymentDto?> UpdatePaymentStatusAsync(int id, PaymentStatus status)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return null;
            }

            payment.PaymentStatus = status;
            if (status == PaymentStatus.Success && payment.PaidAt == null)
            {
                payment.PaidAt = DateTime.UtcNow;
            }

            if (status != PaymentStatus.Success)
            {
                payment.PaidAt = null;
            }

            await _context.SaveChangesAsync();

            return new PaymentDto(payment.Id, payment.OrderId, payment.Amount, payment.PaymentStatus, payment.PaidAt);
        }

        public async Task<bool> DeletePaymentAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return false;
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
