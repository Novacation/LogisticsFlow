using LogisticsFlow.Domain.Repositories;

namespace LogisticsFlow.Application.UseCases.Orders;

public record OrderResponse(
    Guid Id,
    int CustomerId,
    string Destination,
    string Status,
    DateTime CreatedAt,
    DateTime? DispatchedAt,
    List<OrderItemResponse> Items);

public record OrderItemResponse(
    Guid Id,
    string Sku,
    int Quantity);

public interface IGetOrdersUseCase
{
    Task<List<OrderResponse>> ExecuteAsync(CancellationToken cancellationToken = default);
}

public class GetOrdersUseCase(IOrdersRepository ordersRepository) : IGetOrdersUseCase
{
    public async Task<List<OrderResponse>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var orders = await ordersRepository.GetAllAsync(cancellationToken);

        return
        [
            .. orders.Select(order => new OrderResponse(
                order.Id,
                order.CustomerId,
                order.Destination,
                order.Status.ToString(),
                order.CreatedAt,
                order.DispatchedAt,
                [
                    .. order.Items.Select(item => new OrderItemResponse(
                        item.Id,
                        item.Sku,
                        item.Quantity
                    ))
                ]
            ))
        ];
    }
}