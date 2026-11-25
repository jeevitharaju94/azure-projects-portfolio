namespace OrderApi.DTOs
{
    public record CustomerDto(
        int Id,
        string Name,
        string Email);

    public record CreateCustomerDto(
        string Name,
        string Email);

    public record UpdateCustomerDto(
        string Name,
        string Email);
}
