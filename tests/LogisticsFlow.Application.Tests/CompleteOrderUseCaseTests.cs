using LogisticsFlow.Application.UseCases.Orders;
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
}