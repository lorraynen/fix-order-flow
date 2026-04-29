namespace BaseExchange.OrderFlow.OrderGenerator.Application.Common;

/// <summary>
/// Result Pattern para substituir exceções em operações normais
/// Implementa padrão de retorno explícito de erro/sucesso
/// Melhora legibilidade e performance (evita stack traces desnecessários)
/// </summary>
public abstract record Result
{
    /// <summary>
    /// Resultado de sucesso
    /// </summary>
    public sealed record Success : Result;

    /// <summary>
    /// Resultado de falha com mensagens de erro
    /// </summary>
    public sealed record Failure(IReadOnlyList<string> Errors) : Result;

    /// <summary>
    /// Factory para criar sucesso
    /// </summary>
    public static Result AsSuccess() => new Success();

    /// <summary>
    /// Factory para criar falha
    /// </summary>
    public static Result AsFailure(params string[] errors) => 
        new Failure(errors.ToList().AsReadOnly());

    /// <summary>
    /// Factory para criar falha
    /// </summary>
    public static Result AsFailure(IEnumerable<string> errors) => 
        new Failure(errors.ToList().AsReadOnly());

    /// <summary>
    /// Pattern matching para executar ação baseada no resultado
    /// </summary>
    public TResult Match<TResult>(
        Func<Success, TResult> onSuccess,
        Func<Failure, TResult> onFailure) =>
        this switch
        {
            Success s => onSuccess(s),
            Failure f => onFailure(f),
            _ => throw new InvalidOperationException()
        };

    /// <summary>
    /// Pattern matching para executar ação baseada no resultado
    /// </summary>
    public async Task<TResult> MatchAsync<TResult>(
        Func<Success, Task<TResult>> onSuccess,
        Func<Failure, Task<TResult>> onFailure) =>
        this switch
        {
            Success s => await onSuccess(s),
            Failure f => await onFailure(f),
            _ => throw new InvalidOperationException()
        };
}

/// <summary>
/// Result Pattern com dados de retorno
/// </summary>
public abstract record Result<T>
{
    /// <summary>
    /// Resultado de sucesso com dados
    /// </summary>
    public sealed record Success(T Data) : Result<T>;

    /// <summary>
    /// Resultado de falha com mensagens de erro
    /// </summary>
    public sealed record Failure(IReadOnlyList<string> Errors) : Result<T>;

    /// <summary>
    /// Factory para criar sucesso
    /// </summary>
    public static Result<T> AsSuccess(T data) => new Success(data);

    /// <summary>
    /// Factory para criar falha
    /// </summary>
    public static Result<T> AsFailure(params string[] errors) => 
        new Failure(errors.ToList().AsReadOnly());

    /// <summary>
    /// Factory para criar falha
    /// </summary>
    public static Result<T> AsFailure(IEnumerable<string> errors) => 
        new Failure(errors.ToList().AsReadOnly());

    /// <summary>
    /// Pattern matching para executar ação baseada no resultado
    /// </summary>
    public TResult Match<TResult>(
        Func<Success, TResult> onSuccess,
        Func<Failure, TResult> onFailure) =>
        this switch
        {
            Success s => onSuccess(s),
            Failure f => onFailure(f),
            _ => throw new InvalidOperationException()
        };

    /// <summary>
    /// Pattern matching assíncrono
    /// </summary>
    public async Task<TResult> MatchAsync<TResult>(
        Func<Success, Task<TResult>> onSuccess,
        Func<Failure, Task<TResult>> onFailure) =>
        this switch
        {
            Success s => await onSuccess(s),
            Failure f => await onFailure(f),
            _ => throw new InvalidOperationException()
        };

    /// <summary>
    /// Map para transformar sucesso
    /// </summary>
    public Result<TNext> Map<TNext>(Func<T, TNext> mapper) =>
        this switch
        {
            Success s => Result<TNext>.AsSuccess(mapper(s.Data)),
            Failure f => Result<TNext>.AsFailure(f.Errors),
            _ => throw new InvalidOperationException()
        };

    /// <summary>
    /// Bind para transformar sucesso assincronamente
    /// </summary>
    public async Task<Result<TNext>> BindAsync<TNext>(Func<T, Task<Result<TNext>>> binder) =>
        this switch
        {
            Success s => await binder(s.Data),
            Failure f => Result<TNext>.AsFailure(f.Errors),
            _ => throw new InvalidOperationException()
        };
}