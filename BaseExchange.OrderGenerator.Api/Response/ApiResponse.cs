namespace BaseExchange.OrderGenerator.Api.Response
{
    public class ApiResponse<T>
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public T Data { get; private set; }
        public IEnumerable<string> Errors { get; private set; }
        public static ApiResponse<T> SuccessResponse(T data, string message = null)
            => new(true, message ?? "Success", data, null);
        public static ApiResponse<T> ErrorResponse(string message, IEnumerable<string> errors = null)
            => new(false, message, default, errors ?? Enumerable.Empty<string>());
        private ApiResponse(bool success, string message, T data, IEnumerable<string> errors)
        {
            Success = success;
            Message = message;
            Data = data;
            Errors = errors;
        }
    }
}
