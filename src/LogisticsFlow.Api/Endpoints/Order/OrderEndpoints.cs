using LogisticsFlow.Application.UseCases.Orders;
using LogisticsFlow.Domain.Enums;

namespace LogisticsFlow.Api.Endpoints.Order;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/orders");

        group.MapGet("/", GetOrders)
            .WithName(nameof(GetOrders));

        group.MapPost("/", CreateOrder)
            .WithName(nameof(CreateOrder));

        group.MapGet("/{id:guid}", GetOrderById)
            .WithName(nameof(GetOrderById));

        group.MapPost("/{id:guid}/dispatch", BeginDispatchOrder)
            .WithName(nameof(BeginDispatchOrder));

        group.MapPost("/{id:guid}/cancel", CancelOrder)
            .WithName(nameof(CancelOrder));

        group.MapPost("/{id:guid}/complete", CompleteOrder)
            .WithName(nameof(CompleteOrder));
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

    private static async Task<IResult> GetOrders(IGetOrdersUseCase getOrdersUsecase, string? status, string? page,
        string? pageSize, CancellationToken cancellationToken = default)
    {
        OrderStatus? orderStatus = null;

        if (page is null)
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Missing query parameter 'page'",
                detail: "The value for 'page' is required."
            );

        if (!int.TryParse(page, out var pageNumber))
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid query parameter 'page'",
                detail: "The value for 'page' must be at least 1."
            );

        switch (pageNumber)
        {
            case <= 0:
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid query parameter 'page'",
                    detail: "The value for 'page' must be at least 1."
                );
        }

        if (pageSize is null)
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Missing query parameter 'pageSize'",
                detail: "The value for 'pageSize' is required."
            );

        if (!int.TryParse(pageSize, out var pageSizeNumber))
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid query parameter 'pageSize'",
                detail: "The value for 'pageSize' must be at least 5."
            );

        switch (pageSizeNumber)
        {
            case < 5:
            case > 100:
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid query parameter 'pageSize'",
                    detail: "The value for 'pageSize' must be between 5 and 100."
                );
        }


        var offset = ((long)pageNumber - 1) * pageSizeNumber;

        if (offset > int.MaxValue)
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid pagination parameters",
                detail: "The requested page exceeds the supported pagination range."
            );

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

        var orders = await getOrdersUsecase.ExecuteAsync(orderStatus, pageNumber, pageSizeNumber, cancellationToken);
        return Results.Ok(orders);
    }

    private static async Task<IResult> GetOrderById(Guid id,
        IGetOrderByIdUseCase getOrderByIdUsecase,
        CancellationToken cancellationToken = default)
    {
        var order = await getOrderByIdUsecase.ExecuteAsync(id, cancellationToken);

        return Results.Ok(order);
    }

    private static async Task<IResult> BeginDispatchOrder(Guid id, IBeginOrderDispatchUseCase beginOrderDispatchUsecase,
        CancellationToken cancellationToken = default)
    {
        await beginOrderDispatchUsecase.ExecuteAsync(id, cancellationToken);
        return Results.Ok(new
        {
            Id = id,
            Status = nameof(OrderStatus.Processing)
        });
    }

    private static async Task<IResult> CancelOrder(Guid id, ICancelOrderUseCase cancelOrderUseCase,
        CancellationToken cancellationToken = default)
    {
        await cancelOrderUseCase.ExecuteAsync(id, cancellationToken);
        return Results.Ok(new
        {
            Id = id,
            Status = nameof(OrderStatus.Cancelled)
        });
    }

    private static async Task<IResult> CompleteOrder(Guid id, ICompleteOrderUseCase completeOrderUseCase,
        CancellationToken cancellationToken = default)
    {
        await completeOrderUseCase.ExecuteAsync(id, cancellationToken);
        return Results.Ok(new
        {
            Id = id,
            Status = nameof(OrderStatus.Completed)
        });
    }
}