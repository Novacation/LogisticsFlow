using LogisticsFlow.Application.Caching;
using LogisticsFlow.Application.CustomExceptions;
using LogisticsFlow.Application.UseCases.Orders;
using LogisticsFlow.Domain.CustomExceptions;
using LogisticsFlow.Domain.Entities;
using LogisticsFlow.Domain.Enums;
using LogisticsFlow.Domain.Repositories;
using Moq;

namespace LogisticsFlow.Application.Tests;

public class BeginOrderDispatchUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenOrderExists_ShouldBeginDispatchAndSaveChanges()
    {
        var orderId = Guid.NewGuid();
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);

        var cacheMock = new Mock<IOrderCache>();

        cacheMock.Setup(cache => cache.RemoveAsync(orderId, CancellationToken.None)).Returns(Task.CompletedTask);

        var repositoryMock = new Mock<IOrdersRepository>();
        repositoryMock.Setup(repository => repository.GetByIdForUpdateAsync(orderId, CancellationToken.None))
            .ReturnsAsync(order);

        var useCase = new BeginOrderDispatchUseCase(repositoryMock.Object, cacheMock.Object);
        await useCase.ExecuteAsync(orderId, CancellationToken.None);

        Assert.Equal(OrderStatus.Processing, order.Status);

        repositoryMock.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Once);
        cacheMock.Verify(cache => cache.RemoveAsync(orderId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderDoesNotExist_ShouldThrowOrderNotFoundException()
    {
        var orderId = Guid.NewGuid();

        var cacheMock = new Mock<IOrderCache>();

        var repositoryMock = new Mock<IOrdersRepository>();
        repositoryMock.Setup(repository => repository.GetByIdForUpdateAsync(orderId, CancellationToken.None))
            .ReturnsAsync((OrderEntity?)null);

        var useCase = new BeginOrderDispatchUseCase(repositoryMock.Object, cacheMock.Object);
        await Assert.ThrowsAsync<OrderNotFoundException>(() => useCase.ExecuteAsync(orderId, CancellationToken.None));
        repositoryMock.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Never);
        cacheMock.Verify(cache => cache.RemoveAsync(orderId, CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderIsAlreadyProcessing_ShouldThrowInvalidStatusException()
    {
        var orderId = Guid.NewGuid();
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);
        order.BeginDispatch();

        var cacheMock = new Mock<IOrderCache>();

        var repositoryMock = new Mock<IOrdersRepository>();
        repositoryMock.Setup(repository => repository.GetByIdForUpdateAsync(orderId, CancellationToken.None))
            .ReturnsAsync(order);

        var useCase = new BeginOrderDispatchUseCase(repositoryMock.Object, cacheMock.Object);

        await Assert.ThrowsAsync<OrderWithInvalidStatusWhenBeginningDispatchException>(() =>
            useCase.ExecuteAsync(orderId, CancellationToken.None));

        Assert.Equal(OrderStatus.Processing, order.Status);
        repositoryMock.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Never);
        cacheMock.Verify(cache => cache.RemoveAsync(orderId, CancellationToken.None), Times.Never);
    }
}