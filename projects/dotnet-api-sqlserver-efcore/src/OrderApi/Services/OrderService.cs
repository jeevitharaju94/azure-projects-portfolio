using Microsoft.EntityFrameworkCore;
using OrderApi.DTOs;
using OrderApi.Models;

namespace OrderApi.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersAsync()
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapToDto);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order == null ? null : MapToDto(order);
        }

        public async Task<OrderDto> CreateOrderAsync(OrderDto dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
            {
                throw new ArgumentException("At least one order item is required.", nameof(dto));
            }

            var customerExists = await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId);
            if (!customerExists)
            {
                throw new InvalidOperationException("Customer not found.");
            }

            var order = new Order
            {
                CustomerId = dto.CustomerId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = 0m,
                OrderItems = new List<OrderItem>()
            };

            foreach (var itemDto in dto.Items)
            {
                if (itemDto.Quantity <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(dto), "Order quantity must be greater than zero.");
                }

                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == itemDto.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product {itemDto.ProductId} was not found.");
                }

                if (!product.IsActive)
                {
                    throw new InvalidOperationException($"Product {product.Name} is inactive and cannot be ordered.");
                }

                if (product.StockQuantity < itemDto.Quantity)
                {
                    throw new InvalidOperationException($"Not enough stock for product {product.Name}.");
                }

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price,
                    Product = product,
                };

                order.OrderItems.Add(orderItem);
                order.TotalAmount += product.Price * itemDto.Quantity;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var createdOrder = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            return MapToDto(createdOrder!);
        }

        public async Task<OrderDto?> UpdateOrderStatusAsync(int id, OrderStatus status)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return null;
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            return MapToDto(order);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return false;
            }

            if (order.Payment != null)
            {
                throw new InvalidOperationException("Cannot delete an order that already has a payment record.");
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto(
                order.Id,
                order.CustomerId,
                //order.Customer?.Name ?? string.Empty,
                order.OrderDate,
                order.Status,
                order.TotalAmount,
                order.OrderItems.Select(oi => new OrderItemDto(
                    oi.Id,
                    oi.OrderId,
                    oi.ProductId,
                    //oi.Product?.Name ?? string.Empty,
                    oi.Quantity,
                    oi.UnitPrice)).ToList());
        }
    }
}
