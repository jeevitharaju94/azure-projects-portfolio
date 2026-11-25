using OrderApi.Models;

namespace OrderApi.DTOs
{
    public record OrderItemDto(
        int Id,
        int OrderId,
        int ProductId,
        int Quantity,
        decimal UnitPrice);

    public record OrderDto(
        int Id,
        int CustomerId,
        DateTime OrderDate,
        OrderStatus Status,
        decimal TotalAmount,
        List<OrderItemDto> Items);

    public record CreateOrderItemDto(
        int ProductId,
        int Quantity);

    public record CreateOrderDto(
        int CustomerId,
        List<CreateOrderItemDto> Items);
}
