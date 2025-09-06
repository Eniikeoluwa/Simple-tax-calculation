
namespace Nova.Contracts.Models;

public record BaseResponse(bool Success)
{
    public BaseResponse() : this(false)
    { }

    public BaseResponse(string message, bool success = false, List<ErrorResponse>? errors = null) : this(success)
    {
        Message = message;
        Errors = errors;
    }

    public string Message { get; set; } = string.Empty;
    public List<ErrorResponse>? Errors { get; set; }

    public static BaseResponse CreateSuccess(string message = "")
    {
        return new BaseResponse(message, true);
    }

    public static BaseResponse<T> CreateSuccess<T>(T data, string message = "")
    {
        return BaseResponse<T>.CreateSuccess(data, message);
    }

    public static BaseResponse CreateFailure(string message, List<ErrorResponse>? errors = null)
    {
        return new BaseResponse(message, false, errors);
    }

    public static BaseResponse CreateFailure<T>(string message, List<ErrorResponse>? errors = null, T? data = default)
    {
        return BaseResponse<T>.CreateFailure(message, errors, data);
    }
}

public record BaseResponse<T> : BaseResponse
{
    public T Data { get; set; } = default!;

    public static BaseResponse<T> CreateSuccess(T data, string message = "")
    {
        return new BaseResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static BaseResponse<T> CreateFailure(string message, List<ErrorResponse>? errors = null, T? data = default)
    {
        return new BaseResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}

public record ErrorResponse(string Description, string? Code, bool IsValidation = false);