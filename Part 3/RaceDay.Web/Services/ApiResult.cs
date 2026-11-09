namespace RaceDay.Web.Services;

public class ApiResult<T>
{
    public bool IsSuccess { get; init; }
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
}
