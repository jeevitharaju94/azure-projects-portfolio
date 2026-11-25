using Microsoft.AspNetCore.Mvc;
using OrderApi.Controllers;
using OrderApi.DTOs;
using OrderApi.Models;
using OrderApi.Services;
using Xunit;

namespace OrderApi.Tests;

public sealed class ProductsControllerTests
{
    [Fact]
    public async Task GetProducts_ReturnsOkWithProducts()
    {
        var product = new ProductDto(1, "Keyboard", 49.99m, 10, true);
        var service = new FakeProductService { Products = [product] };

        var result = await new ProductsController(service).GetProducts();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal([product], Assert.IsAssignableFrom<IEnumerable<ProductDto>>(ok.Value));
    }

    [Fact]
    public async Task GetProduct_WhenMissing_ReturnsNotFound()
    {
        var result = await new ProductsController(new FakeProductService()).GetProduct(404);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateProduct_WhenValid_ReturnsCreatedAtAction()
    {
        var product = new ProductDto(7, "Mouse", 25m, 5, true);
        var service = new FakeProductService { CreatedProduct = product };

        var result = await new ProductsController(service).CreateProduct(
            new CreateProductDto("Mouse", 25m, 5));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ProductsController.GetProduct), created.ActionName);
        Assert.Equal(product, created.Value);
    }

    [Fact]
    public async Task CreateProduct_WhenInvalid_ReturnsBadRequest()
    {
        var service = new FakeProductService
        {
            CreateException = new ArgumentException("Product name is required.")
        };

        var result = await new ProductsController(service).CreateProduct(
            new CreateProductDto("", 25m, 5));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateProduct_WhenMissing_ReturnsNotFound()
    {
        var result = await new ProductsController(new FakeProductService()).UpdateProduct(
            404, new UpdateProductDto("Missing", 1m, 1, true));

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteProduct_WhenDeleted_ReturnsNoContent()
    {
        var service = new FakeProductService { DeleteResult = true };

        var result = await new ProductsController(service).DeleteProduct(1);

        Assert.IsType<NoContentResult>(result);
    }
}

public sealed class CustomersControllerTests
{
    [Fact]
    public async Task GetCustomers_ReturnsOkWithCustomers()
    {
        var customer = new CustomerDto(1, "Ada", "ada@example.com");
        var service = new FakeCustomerService { Customers = [customer] };

        var result = await new CustomersController(service).GetCustomers();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal([customer], Assert.IsAssignableFrom<IEnumerable<CustomerDto>>(ok.Value));
    }

    [Fact]
    public async Task GetCustomer_WhenMissing_ReturnsNotFound()
    {
        var result = await new CustomersController(new FakeCustomerService()).GetCustomer(404);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateCustomer_WhenValid_ReturnsCreatedAtAction()
    {
        var customer = new CustomerDto(3, "Grace", "grace@example.com");
        var service = new FakeCustomerService { CreatedCustomer = customer };

        var result = await new CustomersController(service).CreateCustomer(
            new CreateCustomerDto("Grace", "grace@example.com"));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(customer, created.Value);
    }

    [Fact]
    public async Task UpdateCustomer_WhenMissing_ReturnsNotFound()
    {
        var result = await new CustomersController(new FakeCustomerService()).UpdateCustomer(
            404, new UpdateCustomerDto("Missing", "missing@example.com"));

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteCustomer_WhenInUse_ReturnsConflict()
    {
        var service = new FakeCustomerService
        {
            DeleteException = new InvalidOperationException("Customer cannot be deleted.")
        };

        var result = await new CustomersController(service).DeleteCustomer(1);

        Assert.IsType<ConflictObjectResult>(result);
    }
}

public sealed class OrdersControllerTests
{
    [Fact]
    public async Task GetOrders_ReturnsOkWithOrders()
    {
        var order = new OrderDto(1, 2, DateTime.UtcNow, OrderStatus.Pending, 50m, []);
        var service = new FakeOrderService { Orders = [order] };

        var result = await new OrdersController(service).GetOrders();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal([order], Assert.IsAssignableFrom<IEnumerable<OrderDto>>(ok.Value));
    }

    [Fact]
    public async Task GetOrder_WhenMissing_ReturnsNotFound()
    {
        var result = await new OrdersController(new FakeOrderService()).GetOrder(404);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateOrder_WhenInvalid_ReturnsBadRequest()
    {
        var service = new FakeOrderService
        {
            CreateException = new InvalidOperationException("Product is not available.")
        };

        var result = await new OrdersController(service).CreateOrder(
            new OrderDto(0, 1, DateTime.UtcNow, OrderStatus.Pending, 0m, []));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateOrderStatus_WhenMissing_ReturnsNotFound()
    {
        var result = await new OrdersController(new FakeOrderService()).UpdateOrderStatus(
            404, OrderStatus.Shipped);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteOrder_WhenCompleted_ReturnsConflict()
    {
        var service = new FakeOrderService
        {
            DeleteException = new InvalidOperationException("Order cannot be deleted.")
        };

        var result = await new OrdersController(service).DeleteOrder(1);

        Assert.IsType<ConflictObjectResult>(result);
    }
}

public sealed class PaymentsControllerTests
{
    [Fact]
    public async Task GetPayments_ReturnsOkWithPayments()
    {
        var payment = new PaymentDto(1, 2, 100m, PaymentStatus.Success, DateTime.UtcNow);
        var service = new FakePaymentService { Payments = [payment] };

        var result = await new PaymentsController(service).GetPayments();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal([payment], Assert.IsAssignableFrom<IEnumerable<PaymentDto>>(ok.Value));
    }

    [Fact]
    public async Task GetPayment_WhenMissing_ReturnsNotFound()
    {
        var result = await new PaymentsController(new FakePaymentService()).GetPayment(404);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreatePayment_WhenValid_ReturnsCreatedAtAction()
    {
        var payment = new PaymentDto(5, 2, 100m, PaymentStatus.Pending, null);
        var service = new FakePaymentService { CreatedPayment = payment };

        var result = await new PaymentsController(service).CreatePayment(
            new CreatePaymentDto(2, 100m, PaymentStatus.Pending));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(payment, created.Value);
    }

    [Fact]
    public async Task CreatePayment_WhenInvalid_ReturnsBadRequest()
    {
        var service = new FakePaymentService
        {
            CreateException = new ArgumentOutOfRangeException("Amount", "Amount must be positive.")
        };

        var result = await new PaymentsController(service).CreatePayment(
            new CreatePaymentDto(2, -1m, PaymentStatus.Pending));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdatePaymentStatus_WhenMissing_ReturnsNotFound()
    {
        var result = await new PaymentsController(new FakePaymentService()).UpdatePaymentStatus(
            404, PaymentStatus.Success);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeletePayment_WhenMissing_ReturnsNotFound()
    {
        var result = await new PaymentsController(new FakePaymentService()).DeletePayment(404);

        Assert.IsType<NotFoundResult>(result);
    }
}

internal sealed class FakeProductService : IProductService
{
    public IEnumerable<ProductDto> Products { get; init; } = [];
    public ProductDto? Product { get; init; }
    public ProductDto? CreatedProduct { get; init; }
    public bool DeleteResult { get; init; }
    public Exception? CreateException { get; init; }

    public Task<IEnumerable<ProductDto>> GetProductsAsync() => Task.FromResult(Products);
    public Task<ProductDto?> GetProductByIdAsync(int id) => Task.FromResult(Product);
    public Task<ProductDto> CreateProductAsync(CreateProductDto dto) =>
        CreateException is null
            ? Task.FromResult(CreatedProduct!)
            : Task.FromException<ProductDto>(CreateException);
    public Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto) =>
        Task.FromResult<ProductDto?>(null);
    public Task<bool> DeleteProductAsync(int id) => Task.FromResult(DeleteResult);
}

internal sealed class FakeCustomerService : ICustomerService
{
    public IEnumerable<CustomerDto> Customers { get; init; } = [];
    public CustomerDto? CreatedCustomer { get; init; }
    public Exception? CreateException { get; init; }
    public Exception? DeleteException { get; init; }

    public Task<IEnumerable<CustomerDto>> GetCustomersAsync() => Task.FromResult(Customers);
    public Task<CustomerDto?> GetCustomerByIdAsync(int id) => Task.FromResult<CustomerDto?>(null);
    public Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto) =>
        CreateException is null
            ? Task.FromResult(CreatedCustomer!)
            : Task.FromException<CustomerDto>(CreateException);
    public Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto dto) =>
        Task.FromResult<CustomerDto?>(null);
    public Task<bool> DeleteCustomerAsync(int id) =>
        DeleteException is null
            ? Task.FromResult(false)
            : Task.FromException<bool>(DeleteException);
}

internal sealed class FakeOrderService : IOrderService
{
    public IEnumerable<OrderDto> Orders { get; init; } = [];
    public Exception? CreateException { get; init; }
    public Exception? DeleteException { get; init; }

    public Task<IEnumerable<OrderDto>> GetOrdersAsync() => Task.FromResult(Orders);
    public Task<OrderDto?> GetOrderByIdAsync(int id) => Task.FromResult<OrderDto?>(null);
    public Task<OrderDto> CreateOrderAsync(OrderDto dto) =>
        CreateException is null
            ? Task.FromResult(dto)
            : Task.FromException<OrderDto>(CreateException);
    public Task<OrderDto?> UpdateOrderStatusAsync(int id, OrderStatus status) =>
        Task.FromResult<OrderDto?>(null);
    public Task<bool> DeleteOrderAsync(int id) =>
        DeleteException is null
            ? Task.FromResult(false)
            : Task.FromException<bool>(DeleteException);
}

internal sealed class FakePaymentService : IPaymentService
{
    public IEnumerable<PaymentDto> Payments { get; init; } = [];
    public PaymentDto? CreatedPayment { get; init; }
    public Exception? CreateException { get; init; }

    public Task<IEnumerable<PaymentDto>> GetPaymentsAsync() => Task.FromResult(Payments);
    public Task<PaymentDto?> GetPaymentByIdAsync(int id) => Task.FromResult<PaymentDto?>(null);
    public Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto) =>
        CreateException is null
            ? Task.FromResult(CreatedPayment!)
            : Task.FromException<PaymentDto>(CreateException);
    public Task<PaymentDto?> UpdatePaymentStatusAsync(int id, PaymentStatus status) =>
        Task.FromResult<PaymentDto?>(null);
    public Task<bool> DeletePaymentAsync(int id) => Task.FromResult(false);
}
