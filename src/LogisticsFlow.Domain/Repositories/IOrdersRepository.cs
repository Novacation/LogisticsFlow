using LogisticsFlow.Domain.Entities;

namespace LogisticsFlow.Domain.Repositories;

public interface IOrdersRepository
{
    Task CreateAsync(OrderEntity orderEntity, CancellationToken cancellationToken = default);
    Task<List<OrderEntity>> GetAllAsync(CancellationToken cancellationToken = default);
}