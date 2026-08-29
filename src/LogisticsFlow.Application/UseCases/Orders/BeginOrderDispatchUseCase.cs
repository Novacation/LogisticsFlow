using LogisticsFlow.Application.CustomExceptions;
using LogisticsFlow.Domain.Repositories;

namespace LogisticsFlow.Application.UseCases.Orders;

public interface IBeginOrderDispatchUseCase
{
    Task ExecuteAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public class BeginOrderDispatchUseCase(IOrdersRepository repository) : IBeginOrderDispatchUseCase
{
    public async Task ExecuteAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await repository.GetByIdForUpdateAsync(orderId, cancellationToken);

        if (order is null) throw new OrderNotFoundException(orderId);

        order.BeginDispatch();

        await repository.SaveChangesAsync(cancellationToken);
    }
}