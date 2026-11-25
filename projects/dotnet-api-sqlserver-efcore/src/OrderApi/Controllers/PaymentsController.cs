using Microsoft.AspNetCore.Mvc;
using OrderApi.DTOs;
using OrderApi.Models;
using OrderApi.Services;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments()
        {
            var payments = await _paymentService.GetPaymentsAsync();
            return Ok(payments);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PaymentDto>> GetPayment(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentDto>> CreatePayment(CreatePaymentDto dto)
        {
            try
            {
                var payment = await _paymentService.CreatePaymentAsync(dto);
                return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] PaymentStatus status)
        {
            var payment = await _paymentService.UpdatePaymentStatusAsync(id, status);
            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var deleted = await _paymentService.DeletePaymentAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
