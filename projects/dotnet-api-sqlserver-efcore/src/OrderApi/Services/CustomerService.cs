using Microsoft.EntityFrameworkCore;
using OrderApi.DTOs;
using OrderApi.Models;

namespace OrderApi.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerDto>> GetCustomersAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CustomerDto(c.Id, c.Name, c.Email))
                .ToListAsync();
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            return customer == null
                ? null
                : new CustomerDto(customer.Id, customer.Name, customer.Email);
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Customer name is required.", nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                throw new ArgumentException("Customer email is required.", nameof(dto));
            }

            var customer = new Customer
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return new CustomerDto(customer.Id, customer.Name, customer.Email);
        }

        public async Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto dto)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Customer name is required.", nameof(dto));
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                throw new ArgumentException("Customer email is required.", nameof(dto));
            }

            customer.Name = dto.Name.Trim();
            customer.Email = dto.Email.Trim();

            await _context.SaveChangesAsync();
            return new CustomerDto(customer.Id, customer.Name, customer.Email);
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return false;
            }

            if (customer.Orders.Any())
            {
                throw new InvalidOperationException("Cannot delete a customer who has existing orders.");
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
