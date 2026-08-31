using LogisticsFlow.Domain.CustomExceptions;
using LogisticsFlow.Domain.Entities;
using LogisticsFlow.Domain.Enums;

namespace LogisticsFlow.Domain.Tests;

public class OrderEntityTests
{
    [Fact]
    public void BeginDispatch_WhenOrderIsCreated_ShouldChangeStatusToProcessing()
    {
        //Arrange
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);

        // Act
        order.BeginDispatch();

        // Assert
        Assert.Equal(OrderStatus.Processing, order.Status);
    }

    [Fact]
    public void BeginDispatch_WhenOrderIsAlreadyProcessing_ShouldThrowInvalidStatusException()
    {
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);

        order.BeginDispatch();

        Assert.Throws<OrderWithInvalidStatusWhenBeginningDispatchException>(order.BeginDispatch);

        Assert.Equal(OrderStatus.Processing, order.Status);
    }

    [Fact]
    public void Dispatch_WhenOrderIsProcessing_ShouldChangeStatusToDispatched()
    {
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);
        order.BeginDispatch();

        order.Dispatch();

        Assert.Equal(OrderStatus.Dispatched, order.Status);
        Assert.NotNull(order.DispatchedAt);
    }

    [Fact]
    public void Dispatch_WhenOrderIsCreated_ShouldThrowInvalidStatusException()
    {
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);

        Assert.Throws<OrderWithInvalidStatusWhenDispatchingException>(order.Dispatch);

        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Null(order.DispatchedAt);
    }

    [Fact]
    public void Complete_WhenOrderIsDispatched_ShouldChangeStatusToCompleted()
    {
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);
        order.BeginDispatch();
        order.Dispatch();
        var dispatchedAtBeforeCompletion = order.DispatchedAt;
        order.Complete();

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(dispatchedAtBeforeCompletion, order.DispatchedAt);
    }

    [Fact]
    public void Complete_WhenOrderIsProcessing_ShouldThrowInvalidStatusException()
    {
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);
        order.BeginDispatch();

        Assert.Throws<OrderWithInvalidStatusWhenCompletingException>(order.Complete);
        Assert.Equal(OrderStatus.Processing, order.Status);
        Assert.Null(order.DispatchedAt);
    }

    [Fact]
    public void Cancel_WhenOrderIsCreated_ShouldChangeStatusToCancelled()
    {
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);

        order.Cancel();
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Null(order.DispatchedAt);
    }

    [Fact]
    public void Cancel_WhenOrderIsProcessing_ShouldChangeStatusToCancelled()
    {
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);

        order.BeginDispatch();

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Null(order.DispatchedAt);
    }
}