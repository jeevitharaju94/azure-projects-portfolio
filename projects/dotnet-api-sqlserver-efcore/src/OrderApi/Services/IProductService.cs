using OrderApi.DTOs;

namespace OrderApi.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto);
        Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteProductAsync(int id);
    }
}
