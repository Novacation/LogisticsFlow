using System.Net;
using System.Net.Http.Json;
using LogisticsFlow.Application.UseCases.Orders;
using LogisticsFlow.Domain.Enums;
using LogisticsFlow.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsFlow.Integration.Tests;

[Collection(nameof(DatabaseIntegrationCollection))]
public class OrderEndpointsTests(MsSqlContainerFixture databaseFixture)
{
    private readonly HttpClient _client =
        databaseFixture.Client;

    [Fact]
    public async Task GetOrders_WhenPageIsZero_ShouldReturnBadRequestProblemDetails()
    {
        var response = await _client.GetAsync("/orders?page=0&pageSize=20");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
    }

    [Fact]
    public async Task CreateOrder_WhenRequestIsValid_ShouldReturnCreated()
    {
        const int customerId = 1;
        const string destination = "Rio de Janeiro";
        const string sku = "SKU-4892";
        const int quantity = 10;

        var orderItems = new List<CreateOrderItemRequest>
        {
            new(sku, quantity)
        };
        var request = new CreateOrderRequest(customerId, destination, orderItems);

        var response = await _client.PostAsJsonAsync("/orders", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task GetOrderById_WhenOrderWasCreated_ShouldReturnPersistedOrder()
    {
        const int customerId = 1;
        const string destination = "Rio de Janeiro";
        const string sku = "SKU-4892";
        const int quantity = 10;

        var orderItems = new List<CreateOrderItemRequest>
        {
            new(sku, quantity)
        };
        var request = new CreateOrderRequest(customerId, destination, orderItems);

        var createResponse = await _client.PostAsJsonAsync("/orders", request);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var getResponse = await _client.GetAsync(createResponse.Headers.Location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var order = await getResponse.Content.ReadFromJsonAsync<GetOrderByIdResponse>();

        Assert.NotNull(order);
        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(customerId, order.CustomerId);
        Assert.Equal(destination, order.Destination);
        Assert.Equal(nameof(OrderStatus.Created), order.Status);
        Assert.NotEqual(default, order.CreatedAt);
        Assert.Null(order.DispatchedAt);

        var responseItem = Assert.Single(order.Items);

        Assert.NotEqual(Guid.Empty, responseItem.Id);
        Assert.Equal(sku, responseItem.Sku);
        Assert.Equal(quantity, responseItem.Quantity);
    }
}