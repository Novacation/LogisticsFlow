using LogisticsFlow.Domain.CustomExceptions;
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

    public void BeginDispatch()
    {
        if (Status != OrderStatus.Created)
            throw new OrderWithInvalidStatusWhenBeginningDispatchException(Status);
        Status = OrderStatus.Processing;
    }

    public void Dispatch()
    {
        if (Status != OrderStatus.Processing)
            throw new OrderWithInvalidStatusWhenDispatchingException(Status);
        Status = OrderStatus.Dispatched;
        DispatchedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Dispatched)
            throw new OrderWithInvalidStatusWhenCompletingException(Status);

        Status = OrderStatus.Completed;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Processing && Status != OrderStatus.Created)
            throw new OrderWithInvalidStatusWhenCancellingException(Status);

        Status = OrderStatus.Cancelled;
    }
}