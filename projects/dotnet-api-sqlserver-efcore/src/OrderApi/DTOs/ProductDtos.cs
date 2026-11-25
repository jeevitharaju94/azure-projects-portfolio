namespace OrderApi.DTOs
{
    public record ProductDto(
        int Id,
        string Name,
        decimal Price,
        int StockQuantity,
        bool IsActive);

    public record CreateProductDto(
        string Name,
        decimal Price,
        int StockQuantity,
        bool IsActive = true);

    public record UpdateProductDto(
        string Name,
        decimal Price,
        int StockQuantity,
        bool IsActive);
}
