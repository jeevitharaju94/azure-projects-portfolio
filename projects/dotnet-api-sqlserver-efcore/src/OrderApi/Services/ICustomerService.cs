using OrderApi.DTOs;

namespace OrderApi.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetCustomersAsync();
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
        Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto dto);
        Task<bool> DeleteCustomerAsync(int id);
    }
}
