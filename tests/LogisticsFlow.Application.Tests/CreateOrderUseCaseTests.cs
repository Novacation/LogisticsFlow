using LogisticsFlow.Application.Caching;
using LogisticsFlow.Application.UseCases.Orders;
using LogisticsFlow.Domain.Entities;
using LogisticsFlow.Domain.Enums;
using LogisticsFlow.Domain.Repositories;
using Moq;

namespace LogisticsFlow.Application.Tests;

public class CreateOrderUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenRequestIsValid_ShouldCreateOrder()
    {
        var createOrderItemsRequest = new List<CreateOrderItemRequest>
        {
            new("SKU-349875", 40)
        };

        var createOrderRequest = new CreateOrderRequest(1, "Rio de Janeiro", createOrderItemsRequest);

        var repositoryMock = new Mock<IOrdersRepository>();

        OrderEntity? capturedOrder = null;

        repositoryMock.Setup(repository => repository.CreateAsync(It.IsAny<OrderEntity>(), CancellationToken.None))
            .Callback<OrderEntity, CancellationToken>((order, _) => capturedOrder = order)
            .Returns(Task.CompletedTask);

        var cacheMock = new Mock<IOrderCache>();
        GetOrderByIdResponse? capturedCacheOrder = null;

        cacheMock.Setup(cache => cache.SetAsync(It.IsAny<GetOrderByIdResponse>(), CancellationToken.None))
            .Callback<GetOrderByIdResponse, CancellationToken>((mappedOrder, _) => capturedCacheOrder = mappedOrder);

        var useCase = new CreateOrderUsecase(repositoryMock.Object, cacheMock.Object);
        var returnedOrderId = await useCase.ExecuteAsync(createOrderRequest, CancellationToken.None);

        Assert.NotNull(capturedOrder);

        Assert.Equal(1, capturedOrder.CustomerId);
        Assert.Equal("Rio de Janeiro", capturedOrder.Destination);
        Assert.Equal(OrderStatus.Created, capturedOrder.Status);

        var capturedItem = Assert.Single(capturedOrder.Items);

        Assert.Equal("SKU-349875", capturedItem.Sku);
        Assert.Equal(40, capturedItem.Quantity);

        Assert.Equal(capturedOrder.Id, returnedOrderId);

        repositoryMock.Verify(repository => repository.CreateAsync(It.IsAny<OrderEntity>(), CancellationToken.None),
            Times.Once);

        Assert.NotNull(capturedCacheOrder);

        Assert.Equal(capturedOrder.Id, capturedCacheOrder.Id);
        Assert.Equal(capturedOrder.Destination, capturedCacheOrder.Destination);
        Assert.Equal(capturedOrder.Status.ToString(), capturedCacheOrder.Status);
        Assert.Equal(capturedOrder.CreatedAt, capturedCacheOrder.CreatedAt);
        Assert.Equal(capturedOrder.CustomerId, capturedCacheOrder.CustomerId);
        Assert.Equal(capturedOrder.DispatchedAt, capturedCacheOrder.DispatchedAt);

        var capturedCacheOrderItem = Assert.Single(capturedCacheOrder.Items);

        Assert.Equal(capturedItem.Id, capturedCacheOrderItem.Id);
        Assert.Equal(capturedItem.Quantity, capturedCacheOrderItem.Quantity);
        Assert.Equal(capturedItem.Sku, capturedCacheOrderItem.Sku);

        cacheMock.Verify(cache => cache.SetAsync(It.IsAny<GetOrderByIdResponse>(), CancellationToken.None), Times.Once);
    }
}