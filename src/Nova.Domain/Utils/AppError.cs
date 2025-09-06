using FluentResults;

namespace Nova.Domain.Utils
{
    public class AppError : Error
    {
        public ErrorType Type { get; set; }
        public string Code { get; set; }

        public AppError(string message, ErrorType type, string code) : base(message)
        {
            Type = type;
            Code = code;
        }

        public static AppError Get(IError error)
        {
            return error as AppError ?? new AppError(error.Message, ErrorType.Unknown, "UNKNOWN");
        }
    }

    public enum ErrorType
    {
        Unknown,
        Conflict,
        Validation,
        NotFound
    }
}
