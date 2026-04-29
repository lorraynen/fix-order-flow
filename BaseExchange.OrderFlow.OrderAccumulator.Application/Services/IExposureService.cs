using BaseExchange.OrderFlow.Domain.Entities;

namespace BaseExchange.OrderFlow.OrderAccumulator.Application.Services;

/// <summary>
/// Define contrato para rastreamento de exposição de ordens
/// Implementa Dependency Inversion Principle (SOLID - D)
/// </summary>
public interface IExposureService
{
    /// <summary>
    /// Processa uma ordem e atualiza a exposição
    /// </summary>
    void ProcessOrder(Order order);

    /// <summary>
    /// Retorna a exposição atual para um símbolo
    /// </summary>
    decimal GetExposure(string symbol);

    /// <summary>
    /// Retorna todas as exposições
    /// </summary>
    IReadOnlyDictionary<string, decimal> GetAll();
}