using LogisticsFlow.Application.UseCases.Orders;
using LogisticsFlow.Domain.Entities;
using LogisticsFlow.Domain.Enums;
using LogisticsFlow.Domain.Repositories;
using Moq;

namespace LogisticsFlow.Application.Tests;

public class GetOrdersUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenOrdersExist_ShouldReturnMappedOrdersAndForwardQueryParameters()
    {
        var order = new OrderEntity(1, "Rio de Janeiro", [new OrderItemEntity("SKU-001", 10)]);
        order.BeginDispatch();
        var orders = new List<OrderEntity>
        {
            order
        };

        var statusFilter = OrderStatus.Processing;
        var pageFilter = 2;
        var pageSizeFilter = 20;

        var repositoryMock = new Mock<IOrdersRepository>();

        repositoryMock.Setup(repository =>
                repository.GetAllReadOnlyAsync(statusFilter, pageFilter, pageSizeFilter, CancellationToken.None))
            .ReturnsAsync(orders);

        var useCase = new GetOrdersUseCase(repositoryMock.Object);

        var response = await useCase.ExecuteAsync(statusFilter, pageFilter, pageSizeFilter, CancellationToken.None);

        var responseOrder = Assert.Single(response);

        Assert.Equal(order.Id, responseOrder.Id);
        Assert.Equal(1, responseOrder.CustomerId);
        Assert.Equal(order.Destination, responseOrder.Destination);
        Assert.Equal(nameof(OrderStatus.Processing), responseOrder.Status);
        Assert.Equal(order.CreatedAt, responseOrder.CreatedAt);

        var orderItem = order.Items.Single();
        var responseOrderItem = Assert.Single(responseOrder.Items);

        Assert.Equal(orderItem.Id, responseOrderItem.Id);
        Assert.Equal(orderItem.Sku, responseOrderItem.Sku);
        Assert.Equal(orderItem.Quantity, responseOrderItem.Quantity);

        repositoryMock.Verify(
            repository =>
                repository.GetAllReadOnlyAsync(statusFilter, pageFilter, pageSizeFilter, CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoOrdersExist_ShouldReturnEmptyList()
    {
        var repositoryMock = new Mock<IOrdersRepository>();

        var pageFilter = 1;
        var pageSizeFilter = 20;

        var emptyOrdersList = new List<OrderEntity>();

        repositoryMock.Setup(repository =>
            repository.GetAllReadOnlyAsync(null, pageFilter, pageSizeFilter, CancellationToken.None)).ReturnsAsync([]);

        var useCase = new GetOrdersUseCase(repositoryMock.Object);
        var response = await useCase.ExecuteAsync(null, pageFilter, pageSizeFilter, CancellationToken.None);

        Assert.Empty(response);
        repositoryMock.Verify(repository =>
            repository.GetAllReadOnlyAsync(null, pageFilter, pageSizeFilter, CancellationToken.None));
    }
}