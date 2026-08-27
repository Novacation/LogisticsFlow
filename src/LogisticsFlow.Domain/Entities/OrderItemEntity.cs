namespace LogisticsFlow.Domain.Entities;

public class OrderItemEntity : BaseEntity
{
    private OrderItemEntity()
    {
    }

    public OrderItemEntity(
        string sku,
        int quantity)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        Sku = sku;
        Quantity = quantity;
    }

    public Guid OrderId { get; private set; }

    public string Sku { get; private set; } = null!;

    public int Quantity { get; private set; }
}