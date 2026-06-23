namespace UserApi.Common;

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public T? Data { get; set; }
    public ServiceErrorType ErrorType { get; set; } = ServiceErrorType.None;

    public static ServiceResult<T> Ok(T data, string message = "")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Message = message,
            Data = data,
            ErrorType = ServiceErrorType.None
        };
    }

    public static ServiceResult<T> Fail(string message, ServiceErrorType errorType)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            ErrorType = errorType
        };
    }
}

public enum ServiceErrorType
{
    None,
    NotFound,
    Conflict,
    BadRequest
}