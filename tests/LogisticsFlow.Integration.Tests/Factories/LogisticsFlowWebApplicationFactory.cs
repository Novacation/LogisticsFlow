using LogisticsFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LogisticsFlow.Integration.Tests.Factories;

public class LogisticsFlowWebApplicationFactory(string connectionString, string redisConnectionString)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "LogisticsFlow:";
            });

            services.RemoveAll<
                IDbContextOptionsConfiguration<LogisticsFlowDbContext>>();

            services.AddDbContext<LogisticsFlowDbContext>(options => options.UseSqlServer(connectionString));
        });
    }
}