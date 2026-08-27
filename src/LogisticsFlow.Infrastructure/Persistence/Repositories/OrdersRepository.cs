using LogisticsFlow.Domain.Entities;
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

    public async Task<List<OrderEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders.AsNoTracking();

        query = query.Include(orderEntity => orderEntity.Items);

        var orders = await query.ToListAsync(cancellationToken);

        return orders;
    }
}