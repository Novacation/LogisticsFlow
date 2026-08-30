using LogisticsFlow.Domain.Entities;
using LogisticsFlow.Domain.Enums;

namespace LogisticsFlow.Domain.Tests;

public class OrderEntityTests
{
    [Fact]
    public void BeginDispatch_WhenOrderIsCreated_ShouldChangeStatusToProcessing()
    {
        //Arrange
        //Crie um OrderEntity válido.

        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);

        // Act
        // Chame BeginDispatch().
        order.BeginDispatch();

        // Assert
        // Compare o status esperado com "order.Status".
        Assert.Equal(OrderStatus.Processing, order.Status);
    }
}