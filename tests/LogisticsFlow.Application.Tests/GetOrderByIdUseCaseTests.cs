using LogisticsFlow.Application.CustomExceptions;
using LogisticsFlow.Application.UseCases.Orders;
using LogisticsFlow.Domain.Entities;
using LogisticsFlow.Domain.Enums;
using LogisticsFlow.Domain.Repositories;
using Moq;

namespace LogisticsFlow.Application.Tests;

public class GetOrderByIdUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenOrderExists_ShouldReturnMappedOrder()
    {
        var orderId = Guid.NewGuid();
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);

        var repositoryMock = new Mock<IOrdersRepository>();

        repositoryMock.Setup(repository => repository.GetByIdReadOnlyAsync(orderId, CancellationToken.None))
            .ReturnsAsync(order);

        var useCase = new GetOrderByIdUseCase(repositoryMock.Object);

        var response = await useCase.ExecuteAsync(orderId, CancellationToken.None);

        var responseItem = Assert.Single(response.Items);

        Assert.Equal(order.CustomerId, response.CustomerId);
        Assert.Equal(order.Destination, response.Destination);
        Assert.Equal(nameof(OrderStatus.Created), response.Status);
        Assert.Equal("SKU-001", responseItem.Sku);
        Assert.Equal(10, responseItem.Quantity);
        Assert.Equal(order.Id, response.Id);
        Assert.Equal(order.CreatedAt, response.CreatedAt);
        Assert.Equal(order.DispatchedAt, response.DispatchedAt);
        Assert.Equal(order.Items[0].Id, responseItem.Id);

        repositoryMock.Verify(repository => repository.GetByIdReadOnlyAsync(orderId, CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderDoesNotExist_ShouldThrowOrderNotFoundException()
    {
        var orderId = Guid.NewGuid();
        var repositoryMock = new Mock<IOrdersRepository>();
        repositoryMock.Setup(repository => repository.GetByIdReadOnlyAsync(orderId, CancellationToken.None))
            .ReturnsAsync((OrderEntity?)null);

        var useCase = new GetOrderByIdUseCase(repositoryMock.Object);

        await Assert.ThrowsAsync<OrderNotFoundException>(() => useCase.ExecuteAsync(orderId, CancellationToken.None));

        repositoryMock.Verify(repository => repository.GetByIdReadOnlyAsync(orderId, CancellationToken.None),
            Times.Once);
    }
}