using System.ComponentModel.DataAnnotations;
using LogisticsFlow.Domain.Entities;
using LogisticsFlow.Domain.Repositories;

namespace LogisticsFlow.Application.UseCases.Orders;

public record CreateOrderRequest(
    [Required(ErrorMessage = "CustomerId is required")]
    int? CustomerId,
    [Required(ErrorMessage = "Destination is required")]
    string? Destination,
    [MinLength(1, ErrorMessage = "There must be at least 1 item")]
    List<CreateOrderItemRequest> Items);

public record CreateOrderItemRequest(
    [Required(ErrorMessage = "SKU is required")]
    string Sku,
    int Quantity);

public interface ICreateOrderUsecase
{
    Task<Guid> ExecuteAsync(CreateOrderRequest orderRequest, CancellationToken cancellationToken = default);
}

public class CreateOrderUsecase(IOrdersRepository ordersRepository) : ICreateOrderUsecase
{
    public async Task<Guid> ExecuteAsync(CreateOrderRequest orderRequest,
        CancellationToken cancellationToken = default)
    {
        var items = orderRequest.Items.Select(x => new OrderItemEntity(x.Sku, x.Quantity)).ToList();
        var order = new OrderEntity(orderRequest.CustomerId!.Value, orderRequest.Destination, items);

        await ordersRepository.CreateAsync(order, cancellationToken);

        return order.Id;
    }
}