using LogisticsFlow.Application.Caching;
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
    public async Task ExecuteAsync_WhenCacheMissesAndOrderExists_ShouldReturnOrderAndPopulateCache()
    {
        var orderId = Guid.NewGuid();
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);

        var repositoryMock = new Mock<IOrdersRepository>();

        repositoryMock.Setup(repository => repository.GetByIdReadOnlyAsync(orderId, CancellationToken.None))
            .ReturnsAsync(order);

        var orderCacheMock = new Mock<IOrderCache>();

        orderCacheMock.Setup(cache => cache.GetByIdAsync(orderId, CancellationToken.None))
            .ReturnsAsync((GetOrderByIdResponse?)null);

        GetOrderByIdResponse? capturedOrder = null;
        orderCacheMock.Setup(cache => cache.SetAsync(It.IsAny<GetOrderByIdResponse>(), CancellationToken.None))
            .Callback<GetOrderByIdResponse, CancellationToken>((cachedOrder, _) => capturedOrder = cachedOrder)
            .Returns(Task.CompletedTask);

        var useCase = new GetOrderByIdUseCase(repositoryMock.Object, orderCacheMock.Object);

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

        Assert.NotNull(capturedOrder);

        Assert.Equal(capturedOrder.Id, response.Id);
        Assert.Equal(capturedOrder.CreatedAt, response.CreatedAt);
        Assert.Equal(capturedOrder.CustomerId, response.CustomerId);
        Assert.Equal(capturedOrder.Destination, response.Destination);
        Assert.Equal(capturedOrder.DispatchedAt, response.DispatchedAt);
        Assert.Equal(capturedOrder.Status, response.Status);

        var cachedOrderItem = Assert.Single(capturedOrder.Items);

        Assert.Equal(cachedOrderItem.Id, responseItem.Id);
        Assert.Equal(cachedOrderItem.Quantity, responseItem.Quantity);
        Assert.Equal(cachedOrderItem.Sku, responseItem.Sku);

        orderCacheMock.Verify(cache => cache.GetByIdAsync(orderId, CancellationToken.None), Times.Once);

        repositoryMock.Verify(repository => repository.GetByIdReadOnlyAsync(orderId, CancellationToken.None),
            Times.Once);

        orderCacheMock.Verify(cache => cache.SetAsync(It.IsAny<GetOrderByIdResponse>(), CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderDoesNotExist_ShouldThrowOrderNotFoundException()
    {
        var orderId = Guid.NewGuid();
        var repositoryMock = new Mock<IOrdersRepository>();
        repositoryMock.Setup(repository => repository.GetByIdReadOnlyAsync(orderId, CancellationToken.None))
            .ReturnsAsync((OrderEntity?)null);

        var orderCacheMock = new Mock<IOrderCache>();

        orderCacheMock.Setup(cache => cache.GetByIdAsync(orderId, CancellationToken.None))
            .ReturnsAsync((GetOrderByIdResponse?)null);

        var useCase = new GetOrderByIdUseCase(repositoryMock.Object, orderCacheMock.Object);

        await Assert.ThrowsAsync<OrderNotFoundException>(() => useCase.ExecuteAsync(orderId, CancellationToken.None));

        orderCacheMock.Verify(cache => cache.GetByIdAsync(orderId, CancellationToken.None), Times.Once);

        repositoryMock.Verify(repository => repository.GetByIdReadOnlyAsync(orderId, CancellationToken.None),
            Times.Once);

        orderCacheMock.Verify(cache => cache.SetAsync(It.IsAny<GetOrderByIdResponse>(), CancellationToken.None),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderExistsInCache_ShouldReturnCachedOrderWithoutCallingRepository()
    {
        //Arrange
        var orderId = Guid.NewGuid();
        var item = new GetOrderItemResponse(Guid.NewGuid(), "SKU-001", 10);

        var cachedGetOrderByIdResponse = new GetOrderByIdResponse(orderId, 1, "Rio de Janeiro",
            nameof(OrderStatus.Created),
            DateTime.UtcNow, null, [item]);

        var orderCacheMock = new Mock<IOrderCache>();

        orderCacheMock.Setup(cache => cache.GetByIdAsync(orderId, CancellationToken.None))
            .ReturnsAsync(cachedGetOrderByIdResponse);

        var repositoryMock = new Mock<IOrdersRepository>();

        var useCase = new GetOrderByIdUseCase(repositoryMock.Object, orderCacheMock.Object);

        var response = await useCase.ExecuteAsync(orderId, CancellationToken.None);

        Assert.Same(cachedGetOrderByIdResponse, response);

        orderCacheMock.Verify(cache => cache.GetByIdAsync(orderId, CancellationToken.None), Times.Once);
        repositoryMock.Verify(repository => repository.GetByIdReadOnlyAsync(orderId, CancellationToken.None),
            Times.Never);
        orderCacheMock.Verify(cache => cache.SetAsync(It.IsAny<GetOrderByIdResponse>(), CancellationToken.None),
            Times.Never);
    }
}