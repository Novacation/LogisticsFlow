using LogisticsFlow.Application.UseCases.Orders;

namespace LogisticsFlow.Application.Caching;

public interface IOrderCache
{
    Task<GetOrderByIdResponse?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);
    Task SetAsync(GetOrderByIdResponse getOrderByIdResponse, CancellationToken cancellationToken);
    Task RemoveAsync(Guid orderId, CancellationToken cancellationToken);
}