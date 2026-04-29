using BaseExchange.OrderFlow.OrderGenerator.Application.DTOs;
using BaseExchange.OrderFlow.Tests.Fixtures;
using BaseExchange.OrderGenerator.Api;
using BaseExchange.OrderFlow.Tests.TestConfiguration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using BaseExchange.OrderFlow.Tests.Integration.Factories;

namespace BaseExchange.OrderFlow.Tests.Integration.Api;

/// <summary>
/// Testes de integração para OrdersController
/// Implementa SOLID: Usa TestWebApplicationFactory para injetar mocks
/// Cenários de teste separados por fixture behavior
/// </summary>
public class OrdersControllerIntegrationTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient? _httpClient;

    public OrdersControllerIntegrationTests()
    {
        // ✅ Usa factory com mocks, não inicializa FIX real
        _factory = TestWebApplicationFactory.CreateWithSuccessfulFix();
    }

    public async Task InitializeAsync()
    {
        _httpClient = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();
        _factory?.Dispose();
        await Task.CompletedTask;
    }

    #region Happy Path Tests

    [Fact]
    public async Task CreateOrder_WithValidRequest_ShouldReturn200OK()
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest();
        using var content = JsonContent.Create(request);

        // Act
        var response = await _httpClient!.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateOrder_WithMultipleValidRequests_ShouldProcessAll()
    {
        // Arrange
        var requests = new[]
        {
            TestFactory.CreateOrderRequest(symbol: "PETR4"),
            TestFactory.CreateOrderRequest(symbol: "VALE3"),
            TestFactory.CreateOrderRequest(symbol: "VIIA4")
        };

        // Act
        var tasks = requests.Select(async req =>
        {
            using var content = JsonContent.Create(req);
            return await _httpClient!.PostAsync("/api/orders", content);
        });

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(r => 
            r.IsSuccessStatusCode.Should().BeTrue(
                $"Expected 2xx but got {r.StatusCode}"));
    }

    #endregion

    #region Bad Request Tests

    [Fact]
    public async Task CreateOrder_WithInvalidSymbol_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(symbol: "INVALID");
        using var content = JsonContent.Create(request);

        // Act
        var response = await _httpClient!.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WithZeroQuantity_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(quantity: 0);
        using var content = JsonContent.Create(request);

        // Act
        var response = await _httpClient!.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WithNegativePrice_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(price: -10.50m);
        using var content = JsonContent.Create(request);

        // Act
        var response = await _httpClient!.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WithEmptySymbol_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest(symbol: "");
        using var content = JsonContent.Create(request);

        // Act
        var response = await _httpClient!.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Concurrent Request Tests

    [Fact]
    public async Task CreateOrder_WithConcurrentRequests_ShouldHandleAll()
    {
        // Arrange
        var requests = Enumerable.Range(0, 5)
            .Select(_ => TestFactory.CreateOrderRequest())
            .ToList();

        // Act
        var tasks = requests.Select(async req =>
        {
            using var content = JsonContent.Create(req);
            return await _httpClient!.PostAsync("/api/orders", content);
        });

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().NotBeEmpty();
        responses.All(r => r.IsSuccessStatusCode)
            .Should().BeTrue();
    }

    #endregion

    #region Response Format Tests

    [Fact]
    public async Task CreateOrder_ResponseShouldHaveCorrectContentType()
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest();
        using var content = JsonContent.Create(request);

        // Act
        var response = await _httpClient!.PostAsync("/api/orders", content);

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    #endregion
}

/// <summary>
/// Testes de integração para cenários de erro (FIX desconectado)
/// Usa collection fixture separada para evitar contaminação entre testes
/// </summary>
public class OrdersControllerErrorScenariosTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient? _httpClient;

    public OrdersControllerErrorScenariosTests()
    {
        // ✅ Factory com FIX desconectado
        _factory = TestWebApplicationFactory.CreateWithDisconnectedFix();
    }

    public async Task InitializeAsync()
    {
        _httpClient = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();
        _factory?.Dispose();
        await Task.CompletedTask;
    }

    #region Service Unavailable Tests

    [Fact]
    public async Task CreateOrder_WhenFixNotConnected_ShouldReturn503ServiceUnavailable()
    {
        // Arrange
        var request = TestFactory.CreateOrderRequest();
        using var content = JsonContent.Create(request);

        // Act
        var response = await _httpClient!.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    #endregion
}