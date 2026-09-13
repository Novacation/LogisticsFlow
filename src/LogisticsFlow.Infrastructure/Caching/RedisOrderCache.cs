using System.Text.Json;
using LogisticsFlow.Application.Caching;
using LogisticsFlow.Application.UseCases.Orders;
using Microsoft.Extensions.Caching.Distributed;

namespace LogisticsFlow.Infrastructure.Caching;

public sealed class RedisOrderCache(IDistributedCache distributedCache) : IOrderCache
{
    public async Task<GetOrderByIdResponse?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await distributedCache.GetStringAsync(GetCacheKey(orderId), cancellationToken);
        return order is null ? null : JsonSerializer.Deserialize<GetOrderByIdResponse>(order);
    }

    public async Task SetAsync(GetOrderByIdResponse getOrderByIdResponse, CancellationToken cancellationToken)
    {
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await distributedCache.SetStringAsync(GetCacheKey(getOrderByIdResponse.Id),
            JsonSerializer.Serialize(getOrderByIdResponse), cacheOptions, cancellationToken);
    }

    public async Task RemoveAsync(Guid orderId, CancellationToken cancellationToken)
    {
        await distributedCache.RemoveAsync(GetCacheKey(orderId), cancellationToken);
    }

    private static string GetCacheKey(Guid orderId)
    {
        return $"order:{orderId}";
    }
}