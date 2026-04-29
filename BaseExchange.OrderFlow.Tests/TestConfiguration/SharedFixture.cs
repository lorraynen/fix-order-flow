using BaseExchange.OrderFlow.OrderAccumulator.Application.Services;
using Xunit;

namespace BaseExchange.OrderFlow.Tests.TestConfiguration;

/// <summary>
/// Fixture compartilhada entre múltiplas classes de teste
/// Implementa SOLID: Usa Factory para criar dependências
/// Depende de abstrações (IExposureService), não de implementações concretas
/// </summary>
public class SharedFixture : IAsyncLifetime
{
    /// <summary>
    /// Serviço de exposição - interface, não implementação
    /// </summary>
    public IExposureService ExposureService { get; private set; } = null!;

    /// <summary>
    /// Inicializa a fixture de forma assíncrona
    /// Usa factory para abstração (static)
    /// </summary>
    public async Task InitializeAsync()
    {
        ExposureService = TestFactory.CreateExposureService();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Limpa a fixture após os testes
    /// </summary>
    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}

/// <summary>
/// Marker class para usar SharedFixture em múltiplas classes de teste
/// Implementa Collection Fixture Pattern do xUnit
/// </summary>
[CollectionDefinition("Exposure Service Collection")]
public class ExposureServiceCollection : ICollectionFixture<SharedFixture>
{
    // Apenas um marcador para associar fixtures
}