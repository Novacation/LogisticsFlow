using LogisticsFlow.Application.CustomExceptions;
using LogisticsFlow.Application.UseCases.Orders;
using LogisticsFlow.Domain.CustomExceptions;
using LogisticsFlow.Domain.Entities;
using LogisticsFlow.Domain.Enums;
using LogisticsFlow.Domain.Repositories;
using Moq;

namespace LogisticsFlow.Application.Tests;

public class CompleteOrderUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenOrderIsDispatched_ShouldCompleteOrderAndSaveChanges()
    {
        var orderId = Guid.NewGuid();
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);

        order.BeginDispatch();
        order.Dispatch();
        var dispatchedAtBeforeComplete = order.DispatchedAt;

        var repositoryMock = new Mock<IOrdersRepository>();
        repositoryMock.Setup(repository => repository.GetByIdForUpdateAsync(orderId, CancellationToken.None))
            .ReturnsAsync(order);

        var useCase = new CompleteOrderUseCase(repositoryMock.Object);

        await useCase.ExecuteAsync(orderId, CancellationToken.None);

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(dispatchedAtBeforeComplete, order.DispatchedAt);
        repositoryMock.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderDoesNotExist_ShouldThrowOrderNotFoundException()
    {
        var orderId = Guid.NewGuid();

        var repositoryMock = new Mock<IOrdersRepository>();
        repositoryMock.Setup(repository => repository.GetByIdForUpdateAsync(orderId, CancellationToken.None))
            .ReturnsAsync((OrderEntity?)null);

        var useCase = new CompleteOrderUseCase(repositoryMock.Object);
        await Assert.ThrowsAsync<OrderNotFoundException>(() => useCase.ExecuteAsync(orderId, CancellationToken.None));

        repositoryMock.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderIsProcessing_ShouldThrowInvalidStatusException()
    {
        var orderId = Guid.NewGuid();
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);
        order.BeginDispatch();

        var repositoryMock = new Mock<IOrdersRepository>();
        repositoryMock.Setup(repository => repository.GetByIdForUpdateAsync(orderId, CancellationToken.None))
            .ReturnsAsync(order);
        var useCase = new CompleteOrderUseCase(repositoryMock.Object);
        await Assert.ThrowsAsync<OrderWithInvalidStatusWhenCompletingException>(() =>
            useCase.ExecuteAsync(orderId, CancellationToken.None));

        repositoryMock.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Never);
        Assert.Equal(OrderStatus.Processing, order.Status);
        Assert.Null(order.DispatchedAt);
    }
}