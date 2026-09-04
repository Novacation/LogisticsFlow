using LogisticsFlow.Infrastructure.Persistence;
using LogisticsFlow.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace LogisticsFlow.Integration.Tests;

[Collection(nameof(DatabaseIntegrationCollection))]
public class DatabaseIntegrationTests(MsSqlContainerFixture databaseFixture)
{
    [Fact]
    public async Task Database_WhenContainerStarts_ShouldApplyMigrationsAndConnect()
    {
        using var scope = databaseFixture.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LogisticsFlowDbContext>();

        var canConnect = await dbContext.Database.CanConnectAsync();

        Assert.True(canConnect);
    }
}