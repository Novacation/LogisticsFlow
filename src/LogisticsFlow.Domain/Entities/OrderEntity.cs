using LogisticsFlow.Domain.Enums;

namespace LogisticsFlow.Domain.Entities;

public class OrderEntity : BaseEntity
{
    // EF Core constructor
    private OrderEntity()
    {
    }

    public OrderEntity(
        int customerId,
        string destination,
        List<OrderItemEntity> items)
    {
        CustomerId = customerId;
        Destination = destination;
        Items = items;

        Status = OrderStatus.Created;
        CreatedAt = DateTime.UtcNow;
    }

    public int CustomerId { get; private set; }

    public string Destination { get; private set; }

    public OrderStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? DispatchedAt { get; private set; }

    public List<OrderItemEntity> Items { get; private set; } = [];
}