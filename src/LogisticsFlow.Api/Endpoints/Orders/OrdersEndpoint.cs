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

        group.MapGet("/{id:guid}", GetOrderById)
            .WithName("GetOrderById");
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

    private static async Task<IResult> GetOrders(IGetOrdersUseCase getOrdersUsecase, string? status,
        int? page = 1,
        int? pageSize = 5, CancellationToken cancellationToken = default)
    {
        OrderStatus? orderStatus = null;

        switch (page)
        {
            case null:
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Missing query parameter 'page'",
                    detail: "The value for 'page' is required."
                );
            case <= 0:
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid query parameter 'page'",
                    detail: "The value for 'page' must be at least 1."
                );
        }


        switch (pageSize)
        {
            case null:
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Missing query parameter 'pageSize'",
                    detail: "The value for 'pageSize' is required."
                );
            case < 5:
            case > 100:
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid query parameter 'pageSize'",
                    detail: "The value for 'pageSize' must be between 5 and 100."
                );
        }


        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<OrderStatus>(status, true, out var parsedStatus) ||
                !Enum.IsDefined(parsedStatus) || int.TryParse(status, out _))
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid query parameter 'status'",
                    detail: $"The value '{status}' is not valid for 'status'.",
                    extensions: new Dictionary<string, object?>
                    {
                        ["errors"] = new
                        {
                            status = new[]
                            {
                                $"Invalid status. Valid values are: {string.Join(", ", Enum.GetNames<OrderStatus>())}"
                            }
                        }
                    }
                );

            orderStatus = parsedStatus;
        }

        var orders = await getOrdersUsecase.ExecuteAsync(orderStatus, page.Value, pageSize.Value, cancellationToken);
        return Results.Ok(orders);
    }

    private static async Task<IResult> GetOrderById(Guid id,
        IGetOrderByIdUseCase getOrderByIdUsecase,
        CancellationToken cancellationToken = default)
    {
        var order = await getOrderByIdUsecase.ExecuteAsync(id, cancellationToken);
        return order is null ? Results.NotFound() : Results.Ok(order);
    }

    private static async Task<IResult> DispatchOrder()
    {
        return Results.Ok();
    }
}