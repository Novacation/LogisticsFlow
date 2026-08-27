using LogisticsFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogisticsFlow.Infrastructure.Persistence;

public class LogisticsFlowDbContext(DbContextOptions<LogisticsFlowDbContext> options) : DbContext(options)
{
    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderItemEntity> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LogisticsFlowDbContext).Assembly);
    }
}