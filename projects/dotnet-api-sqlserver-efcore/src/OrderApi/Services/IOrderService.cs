using OrderApi.DTOs;
using OrderApi.Models;

namespace OrderApi.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<OrderDto> CreateOrderAsync(OrderDto dto);
        Task<OrderDto?> UpdateOrderStatusAsync(int id, OrderStatus status);
        Task<bool> DeleteOrderAsync(int id);
    }
}
