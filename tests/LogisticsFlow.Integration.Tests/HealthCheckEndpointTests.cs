using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogisticsFlow.Integration.Tests;

public class HealthCheckEndpointTests(WebApplicationFactory<Program> webApplicationFactory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = webApplicationFactory.CreateClient();

    [Fact]
    public async Task GetHealthCheck_WhenApplicationIsRunning_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/health-check");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}