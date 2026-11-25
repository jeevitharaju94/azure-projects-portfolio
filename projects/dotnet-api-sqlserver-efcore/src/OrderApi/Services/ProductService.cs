using Microsoft.EntityFrameworkCore;
using OrderApi.DTOs;
using OrderApi.Models;

namespace OrderApi.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new ProductDto(
                    p.Id,
                    p.Name,
                    p.Price,
                    p.StockQuantity,
                    p.IsActive))
                .ToListAsync();
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return product == null
                ? null
                : new ProductDto(product.Id, product.Name, product.Price, product.StockQuantity, product.IsActive);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Product name is required.", nameof(dto));
            }

            if (dto.Price < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dto), "Price cannot be negative.");
            }

            if (dto.StockQuantity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dto), "Stock quantity cannot be negative.");
            }

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                IsActive = dto.IsActive
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return new ProductDto(product.Id, product.Name, product.Price, product.StockQuantity, product.IsActive);
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Product name is required.", nameof(dto));
            }

            if (dto.Price < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dto), "Price cannot be negative.");
            }

            if (dto.StockQuantity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dto), "Stock quantity cannot be negative.");
            }

            product.Name = dto.Name.Trim();
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return new ProductDto(product.Id, product.Name, product.Price, product.StockQuantity, product.IsActive);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
