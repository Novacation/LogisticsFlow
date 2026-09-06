using System.Data.Common;
using LogisticsFlow.Infrastructure.Persistence;
using LogisticsFlow.Integration.Tests.Factories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;
using Testcontainers.MsSql;

namespace LogisticsFlow.Integration.Tests.Fixtures;

public sealed class MsSqlContainerFixture : IAsyncLifetime
{
    private const string MsSqlImage =
        "mcr.microsoft.com/mssql/server:2022-latest";

    private readonly MsSqlContainer _container = new MsSqlBuilder(MsSqlImage).Build();

    private DbConnection _dbConnection = null!;

    private Respawner _respawner = null!;

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
        _dbConnection = new SqlConnection(ConnectionString);
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore =
            [
                new Table("__EFMigrationsHistory")
            ]
        });

        Client = Factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
        await _dbConnection.DisposeAsync();
        await _container.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }
}