using LogisticsFlow.Application.CustomExceptions;
using LogisticsFlow.Domain.CustomExceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace LogisticsFlow.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case OrderNotFoundException:
                await Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Order not found",
                    detail: exception.Message
                ).ExecuteAsync(httpContext);
                break;

            case OrderWithInvalidStatusWhenBeginningDispatchException:
            case OrderWithInvalidStatusWhenDispatchingException:
            case OrderWithInvalidStatusWhenCompletingException:
            case OrderWithInvalidStatusWhenCancellingException:
                await Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Invalid order process.",
                    detail: exception.Message
                ).ExecuteAsync(httpContext);
                break;

            case OperationCanceledException
                when httpContext.RequestAborted.IsCancellationRequested:
                return true;

            default:
                logger.LogError(exception, "An unexpected error occurred while processing the request.");

                await Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Unexpected error.",
                    detail: "An unexpected error has occurred."
                ).ExecuteAsync(httpContext);
                break;
        }

        return true;
    }
}