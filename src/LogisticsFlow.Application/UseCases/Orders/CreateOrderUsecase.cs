using LogisticsFlow.Domain.Entities;
using LogisticsFlow.Domain.Repositories;

namespace LogisticsFlow.Application.UseCases.Orders;

public record CreateOrderRequest(
    int CustomerId,
    string Destination,
    List<CreateOrderItemRequest> Items);

public record CreateOrderItemRequest(
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
        var order = new OrderEntity(orderRequest.CustomerId, orderRequest.Destination, items);

        await ordersRepository.CreateAsync(order, cancellationToken);

        return order.Id;
    }
}