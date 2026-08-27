using LogisticsFlow.Application.UseCases.Orders;
using LogisticsFlow.Domain.Enums;

namespace LogisticsFlow.Api.Endpoints.Orders;

public static class OrdersEndpoint
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/orders");

        group.MapGet("/", GetOrders)
            .WithName("GetOrders");

        group.MapPost("/", CreateOrder)
            .WithName("CreateOrder");
    }

    private static async Task<IResult> CreateOrder(CreateOrderRequest request, ICreateOrderUsecase createOrderUsecase,
        CancellationToken cancellationToken = default)
    {
        var orderId = await createOrderUsecase.ExecuteAsync(request, cancellationToken);
        return Results.Created($"/orders/{orderId}", new
        {
            Id = orderId,
            Status = nameof(OrderStatus.Created)
        });
    }

    private static async Task<IResult> GetOrders(IGetOrdersUseCase getOrdersUsecase,
        CancellationToken cancellationToken = default)
    {
        var orders = await getOrdersUsecase.ExecuteAsync(cancellationToken);
        return Results.Ok(orders);
    }
}