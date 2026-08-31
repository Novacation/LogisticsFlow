using LogisticsFlow.Application.CustomExceptions;
using LogisticsFlow.Application.UseCases.Orders;
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

        var repositoryMock = new Mock<IOrdersRepository>();
        repositoryMock.Setup(repository => repository.GetByIdForUpdateAsync(orderId, CancellationToken.None))
            .ReturnsAsync(order);

        var useCase = new BeginOrderDispatchUseCase(repositoryMock.Object);
        await useCase.ExecuteAsync(orderId, CancellationToken.None);

        Assert.Equal(OrderStatus.Processing, order.Status);

        repositoryMock.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderDoesNotExist_ShouldThrowOrderNotFoundException()
    {
        var orderId = Guid.NewGuid();
        var repositoryMock = new Mock<IOrdersRepository>();
        repositoryMock.Setup(repository => repository.GetByIdForUpdateAsync(orderId, CancellationToken.None))
            .ReturnsAsync((OrderEntity?)null);

        var useCase = new BeginOrderDispatchUseCase(repositoryMock.Object);
        await Assert.ThrowsAsync<OrderNotFoundException>(() => useCase.ExecuteAsync(orderId, CancellationToken.None));
        repositoryMock.Verify(repository => repository.SaveChangesAsync(CancellationToken.None), Times.Never);
    }
}