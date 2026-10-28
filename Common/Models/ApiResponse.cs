namespace QLSK.Common.Models;
public record ApiResponse<T>(bool Success, T? Data, string? Code, string? Message)
{
    public static ApiResponse<T> Ok(T data) => new(true, data, null, null);
    public static ApiResponse<T> Fail(string code, string message) => new(false, default, code, message);
}
public static class ApiResponse
{
    public static ApiResponse<T> Ok<T>(T data) => ApiResponse<T>.Ok(data);
    public static ApiResponse<object> Fail(string code, string message) => ApiResponse<object>.Fail(code, message);
}
