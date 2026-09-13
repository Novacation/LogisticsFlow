using LogisticsFlow.Application.Caching;
using LogisticsFlow.Application.CustomExceptions;
using LogisticsFlow.Domain.Repositories;

namespace LogisticsFlow.Application.UseCases.Orders;

public record GetOrderByIdResponse(
    Guid Id,
    int CustomerId,
    string Destination,
    string Status,
    DateTime CreatedAt,
    DateTime? DispatchedAt,
    IReadOnlyCollection<GetOrderItemResponse> Items);

public interface IGetOrderByIdUseCase
{
    Task<GetOrderByIdResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class GetOrderByIdUseCase(IOrdersRepository repository, IOrderCache orderCache) : IGetOrderByIdUseCase
{
    public async Task<GetOrderByIdResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cachedOrder = await orderCache.GetByIdAsync(id, cancellationToken);
        if (cachedOrder != null) return cachedOrder;

        var order = await repository.GetByIdReadOnlyAsync(id, cancellationToken);
        if (order is null) throw new OrderNotFoundException(id);

        var mappedOrder = new GetOrderByIdResponse
        (
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
        );

        await orderCache.SetAsync(mappedOrder, cancellationToken);

        return mappedOrder;
    }
}