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

        var useCase = new CreateOrderUsecase(repositoryMock.Object);
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
    }
}