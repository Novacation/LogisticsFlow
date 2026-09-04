using LogisticsFlow.Infrastructure.Persistence;
using LogisticsFlow.Integration.Tests.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace LogisticsFlow.Integration.Tests.Fixtures;

public sealed class MsSqlContainerFixture : IAsyncLifetime
{
    private const string MsSqlImage =
        "mcr.microsoft.com/mssql/server:2022-latest";

    private readonly MsSqlContainer _container = new MsSqlBuilder(MsSqlImage).Build();

    private string ConnectionString => _container.GetConnectionString();

    public HttpClient Client { get; private set; } = null!;
    public LogisticsFlowWebApplicationFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Factory = new LogisticsFlowWebApplicationFactory(ConnectionString);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LogisticsFlowDbContext>();

        //search and apply migrations located at the same assembly of the dbcontext (LogisticsFlowDbContext)
        await dbContext.Database.MigrateAsync();

        Client = Factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LogisticsFlowDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
    }
}