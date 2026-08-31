using LogisticsFlow.Application.CustomExceptions;
using LogisticsFlow.Application.UseCases.Orders;
using LogisticsFlow.Domain.CustomExceptions;
using LogisticsFlow.Domain.Entities;
using LogisticsFlow.Domain.Enums;
using LogisticsFlow.Domain.Repositories;
using Moq;

namespace LogisticsFlow.Application.Tests;

public class CancelOrderUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenOrderCanBeCancelled_ShouldCancelOrderAndSaveChanges()
    {
        var orderId = Guid.NewGuid();
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);

        var repositoryMock = new Mock<IOrdersRepository>();
        repositoryMock.Setup(repository => repository.GetByIdForUpdateAsync(orderId, CancellationToken.None))
            .ReturnsAsync(order);

        var useCase = new CancelOrderUseCase(repositoryMock.Object);
        await useCase.ExecuteAsync(orderId, CancellationToken.None);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        repositoryMock.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderDoesNotExist_ShouldThrowOrderNotFoundException()
    {
        var orderId = Guid.NewGuid();

        var repositoryMock = new Mock<IOrdersRepository>();
        repositoryMock.Setup(repository => repository.GetByIdForUpdateAsync(orderId, CancellationToken.None))
            .ReturnsAsync((OrderEntity?)null);

        var useCase = new CancelOrderUseCase(repositoryMock.Object);
        await Assert.ThrowsAsync<OrderNotFoundException>(() => useCase.ExecuteAsync(orderId, CancellationToken.None));

        repositoryMock.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderIsDispatched_ShouldThrowInvalidStatusException()
    {
        var orderId = Guid.NewGuid();
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);

        order.BeginDispatch();
        order.Dispatch();
        var dispatchedAtBeforeCancelling = order.DispatchedAt;

        var repositoryMock = new Mock<IOrdersRepository>();
        repositoryMock.Setup(repository => repository.GetByIdForUpdateAsync(orderId, CancellationToken.None))
            .ReturnsAsync(order);

        var useCase = new CancelOrderUseCase(repositoryMock.Object);
        await Assert.ThrowsAsync<OrderWithInvalidStatusWhenCancellingException>(() =>
            useCase.ExecuteAsync(orderId, CancellationToken.None));

        Assert.Equal(OrderStatus.Dispatched, order.Status);
        Assert.Equal(dispatchedAtBeforeCancelling, order.DispatchedAt);
        repositoryMock.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Never);
    }
}