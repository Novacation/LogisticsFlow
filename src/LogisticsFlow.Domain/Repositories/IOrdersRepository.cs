using LogisticsFlow.Domain.Entities;
using LogisticsFlow.Domain.Enums;

namespace LogisticsFlow.Domain.Repositories;

public interface IOrdersRepository
{
    Task CreateAsync(OrderEntity orderEntity, CancellationToken cancellationToken = default);

    Task<List<OrderEntity>> GetAllReadOnlyAsync(OrderStatus? status, int page = 1, int pageSize = 5,
        CancellationToken cancellationToken = default);

    Task<OrderEntity?> GetByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OrderEntity?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}