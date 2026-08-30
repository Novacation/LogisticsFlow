using LogisticsFlow.Application.CustomExceptions;
using LogisticsFlow.Domain.Repositories;

namespace LogisticsFlow.Application.UseCases.Orders;

public interface ICompleteOrderUseCase
{
    Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class CompleteOrderUseCase(IOrdersRepository repository) : ICompleteOrderUseCase
{
    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await repository.GetByIdForUpdateAsync(id, cancellationToken);

        if (order is null) throw new OrderNotFoundException(id);

        order.Complete();

        await repository.SaveChangesAsync(cancellationToken);
    }
}