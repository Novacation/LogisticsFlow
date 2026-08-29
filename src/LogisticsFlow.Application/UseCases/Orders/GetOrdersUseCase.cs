using LogisticsFlow.Domain.Enums;
using LogisticsFlow.Domain.Repositories;

namespace LogisticsFlow.Application.UseCases.Orders;

public record GetOrdersResponse(
    Guid Id,
    int CustomerId,
    string Destination,
    string Status,
    DateTime CreatedAt,
    DateTime? DispatchedAt,
    IReadOnlyCollection<GetOrderItemResponse> Items);

public record GetOrderItemResponse(
    Guid Id,
    string Sku,
    int Quantity);

public interface IGetOrdersUseCase
{
    Task<List<GetOrdersResponse>> ExecuteAsync(OrderStatus? status,
        int page = 1, int pageSize = 5,
        CancellationToken cancellationToken = default);
}

public class GetOrdersUseCase(IOrdersRepository ordersRepository) : IGetOrdersUseCase
{
    public async Task<List<GetOrdersResponse>> ExecuteAsync(
        OrderStatus? status,
        int page = 1, int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        var orders = await ordersRepository.GetAllAsync(status, page, pageSize, cancellationToken);

        return
        [
            .. orders.Select(order => new GetOrdersResponse(
                order.Id,
                order.CustomerId,
                order.Destination,
                order.Status.ToString(),
                order.CreatedAt,
                order.DispatchedAt,
                [
                    .. order.Items.Select(item => new GetOrderItemResponse(
                        item.Id,
                        item.Sku,
                        item.Quantity
                    ))
                ]
            ))
        ];
    }
}