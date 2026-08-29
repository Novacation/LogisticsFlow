using LogisticsFlow.Domain.Entities;
using LogisticsFlow.Domain.Enums;
using LogisticsFlow.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LogisticsFlow.Infrastructure.Persistence.Repositories;

public class OrdersRepository(LogisticsFlowDbContext dbContext) : IOrdersRepository
{
    public async Task CreateAsync(OrderEntity orderEntity, CancellationToken cancellationToken = default)
    {
        dbContext.Orders.Add(orderEntity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<OrderEntity>> GetAllReadOnlyAsync(OrderStatus? status, int page = 1, int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders.AsNoTracking();

        if (status is { } orderStatus) query = query.Where(x => x.Status == orderStatus);


        query = query
            .OrderByDescending(order => order.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(orderEntity => orderEntity.Items);

        var orders = await query.ToListAsync(cancellationToken);

        return orders;
    }

    public async Task<OrderEntity?> GetByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders.AsNoTracking().Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return order;
    }

    public async Task<OrderEntity?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return order;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}