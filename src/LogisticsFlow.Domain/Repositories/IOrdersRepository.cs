using LogisticsFlow.Domain.Entities;
using LogisticsFlow.Domain.Enums;

namespace LogisticsFlow.Domain.Repositories;

public interface IOrdersRepository
{
    Task CreateAsync(OrderEntity orderEntity, CancellationToken cancellationToken = default);

    Task<List<OrderEntity>> GetAllAsync(OrderStatus? status, int page = 1, int pageSize = 5,
        CancellationToken cancellationToken = default);

    Task<OrderEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}