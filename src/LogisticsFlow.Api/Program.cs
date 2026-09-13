using LogisticsFlow.Api.Endpoints.Order;
using LogisticsFlow.Api.ExceptionHandling;
using LogisticsFlow.Application.Caching;
using LogisticsFlow.Application.UseCases.Orders;
using LogisticsFlow.Domain.Repositories;
using LogisticsFlow.Infrastructure.Caching;
using LogisticsFlow.Infrastructure.Persistence;
using LogisticsFlow.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();
builder.Services.AddScoped<ICreateOrderUsecase, CreateOrderUsecase>();
builder.Services.AddScoped<IGetOrdersUseCase, GetOrdersUseCase>();
builder.Services.AddScoped<IGetOrderByIdUseCase, GetOrderByIdUseCase>();
builder.Services.AddScoped<IBeginOrderDispatchUseCase, BeginOrderDispatchUseCase>();
builder.Services.AddScoped<ICancelOrderUseCase, CancelOrderUseCase>();
builder.Services.AddScoped<ICompleteOrderUseCase, CompleteOrderUseCase>();
builder.Services.AddScoped<IOrderCache, RedisOrderCache>();

var connectionString = builder.Configuration.GetConnectionString("LogisticsFlowDbStringConnection");
builder.Services.AddDbContext<LogisticsFlowDbContext>(options => options.UseSqlServer(connectionString));

var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "LogisticsFlow:";
});

builder.Services.AddValidation();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.MapOrderEndpoints();

if (app.Environment.IsDevelopment())
    //health-check
    app.MapGet("/health-check", () => Results.Ok());

app.Run();


//needed for integration tests
public partial class Program;