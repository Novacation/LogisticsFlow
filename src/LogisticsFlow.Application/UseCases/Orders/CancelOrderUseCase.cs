using LogisticsFlow.Application.CustomExceptions;
using LogisticsFlow.Domain.Repositories;

namespace LogisticsFlow.Application.UseCases.Orders;

public interface ICancelOrderUseCase
{
    Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class CancelOrderUseCase(IOrdersRepository repository) : ICancelOrderUseCase
{
    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await repository.GetByIdForUpdateAsync(id, cancellationToken);

        if (order is null) throw new OrderNotFoundException(id);

        order.Cancel();

        await repository.SaveChangesAsync(cancellationToken);
    }
}