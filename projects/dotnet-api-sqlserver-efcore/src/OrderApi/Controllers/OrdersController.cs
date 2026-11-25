using Microsoft.AspNetCore.Mvc;
using OrderApi.DTOs;
using OrderApi.Models;
using OrderApi.Services;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var orders = await _orderService.GetOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(OrderDto dto)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(dto);
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatus status)
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, status);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var deleted = await _orderService.DeleteOrderAsync(id);
                if (!deleted)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
