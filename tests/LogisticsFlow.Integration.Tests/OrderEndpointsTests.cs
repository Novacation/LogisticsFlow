using System.Net;
using System.Net.Http.Json;
using LogisticsFlow.Application.UseCases.Orders;
using LogisticsFlow.Domain.Enums;
using LogisticsFlow.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsFlow.Integration.Tests;

[Collection(nameof(DatabaseIntegrationCollection))]
public class OrderEndpointsTests(MsSqlContainerFixture databaseFixture) : IAsyncLifetime
{
    private readonly HttpClient _client =
        databaseFixture.Client;

    public Task InitializeAsync()
    {
        return databaseFixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Theory]
    [InlineData("/orders?page=0&pageSize=20")]
    [InlineData("/orders?pageSize=20")]
    [InlineData("/orders?page=t&pageSize=20")]
    [InlineData("/orders?page=1")]
    [InlineData("/orders?page=1&pageSize=t")]
    [InlineData("/orders?page=1&pageSize=4")]
    [InlineData("/orders?page=1&pageSize=101")]
    public async Task GetOrders_WhenPaginationIsInvalid_ShouldReturnBadRequestProblemDetails(string requestUri)
    {
        var response = await _client.GetAsync(requestUri);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
    }

    [Fact]
    public async Task GetOrders_WhenOffsetIsBiggerThanIntMaxValue_ShouldReturnBadRequestProblemDetails()
    {
        var response = await _client.GetAsync("/orders?page=2147483647&pageSize=100");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
    }

    [Fact]
    public async Task GetOrders_WhenStatusIsValid_ShouldReturnOrders()
    {
        var response = await _client.GetAsync("/orders?page=1&pageSize=20&status=Created");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content.ReadFromJsonAsync<List<GetOrdersResponse>>();

        Assert.NotNull(orders);
    }

    [Theory]
    [InlineData("/orders?page=1&pageSize=20&status=1")]
    [InlineData("/orders?page=1&pageSize=20&status=invalidStatus")]
    public async Task GetOrders_WhenStatusIsInvalid_ShouldReturnBadRequestProblemDetails(string requestUri)
    {
        var response = await _client.GetAsync(requestUri);

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

    [Fact]
    public async Task ResetDatabaseAsync_WhenOrderExists_ShouldRemoveOrderData()
    {
        await CreateValidOrderAsync();

        await databaseFixture.ResetDatabaseAsync();

        var responseOrders = await _client.GetAsync("/orders?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, responseOrders.StatusCode);

        var getOrdersResponse = await responseOrders.Content.ReadFromJsonAsync<List<GetOrdersResponse>>();

        Assert.NotNull(getOrdersResponse);
        Assert.Empty(getOrdersResponse);
    }

    [Fact]
    public async Task BeginDispatch_WhenOrderIsCreated_ShouldPersistProcessingStatus()
    {
        var location = await CreateValidOrderAsync();

        var responseOrderDispatch = await _client.PostAsync($"{location}/dispatch", null);

        Assert.Equal(HttpStatusCode.OK, responseOrderDispatch.StatusCode);

        var responseOrder = await _client.GetAsync(location);

        Assert.Equal(HttpStatusCode.OK, responseOrder.StatusCode);

        var getOrderByIdResponse = await responseOrder.Content.ReadFromJsonAsync<GetOrderByIdResponse>();

        Assert.NotNull(getOrderByIdResponse);

        Assert.Equal(nameof(OrderStatus.Processing), getOrderByIdResponse.Status);
        Assert.Null(getOrderByIdResponse.DispatchedAt);
    }

    [Fact]
    public async Task BeginDispatch_WhenOrderIsAlreadyProcessing_ShouldReturnConflictProblemDetails()
    {
        var location = await CreateValidOrderAsync();

        var responseOrderDispatch = await _client.PostAsync($"{location}/dispatch", null);

        Assert.Equal(HttpStatusCode.OK, responseOrderDispatch.StatusCode);

        responseOrderDispatch = await _client.PostAsync($"{location}/dispatch", null);

        Assert.Equal(HttpStatusCode.Conflict, responseOrderDispatch.StatusCode);
        Assert.Equal("application/problem+json", responseOrderDispatch.Content.Headers.ContentType?.MediaType);

        var problemDetails = await responseOrderDispatch.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.Conflict, problemDetails.Status);
        Assert.Equal("Invalid order process.", problemDetails.Title);
    }

    [Fact]
    public async Task BeginDispatch_WhenOrderDoesNotExist_ShouldReturnNotFoundProblemDetails()
    {
        var orderId = Guid.NewGuid();

        var responseOrderDispatch = await _client.PostAsync($"/orders/{orderId}/dispatch", null);

        Assert.Equal(HttpStatusCode.NotFound, responseOrderDispatch.StatusCode);
        Assert.Equal("application/problem+json", responseOrderDispatch.Content.Headers.ContentType?.MediaType);

        var problemDetails = await responseOrderDispatch.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.NotFound, problemDetails.Status);
        Assert.Equal("Order not found", problemDetails.Title);
    }

    [Fact]
    public async Task CancelOrder_WhenOrderIsCreated_ShouldPersistCancelledStatus()
    {
        var location = await CreateValidOrderAsync();
        var responseOrderCancel = await _client.PostAsync($"{location}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, responseOrderCancel.StatusCode);

        var responseOrder = await _client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, responseOrder.StatusCode);

        var getOrderByIdResponse = await responseOrder.Content.ReadFromJsonAsync<GetOrderByIdResponse>();
        Assert.NotNull(getOrderByIdResponse);
        Assert.Equal(nameof(OrderStatus.Cancelled), getOrderByIdResponse.Status);
        Assert.Null(getOrderByIdResponse.DispatchedAt);
    }

    private async Task<Uri> CreateValidOrderAsync()
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
        var responseOrderCreation = await _client.PostAsJsonAsync("/orders", request);

        var location = responseOrderCreation.Headers.Location;
        Assert.Equal(HttpStatusCode.Created, responseOrderCreation.StatusCode);
        Assert.NotNull(location);

        return location;
    }
}