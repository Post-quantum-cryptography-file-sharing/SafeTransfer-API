namespace SafeTransfer_API.Services;

public enum ServiceStatus { Ok, NotFound, Forbidden, Invalid }

public record ServiceResult<T>(ServiceStatus Status, T? Value = default, string? Error = null)
{
    public static ServiceResult<T> Ok(T value) => new(ServiceStatus.Ok, value);
    public static ServiceResult<T> NotFound() => new(ServiceStatus.NotFound);
    public static ServiceResult<T> Forbidden() => new(ServiceStatus.Forbidden);
    public static ServiceResult<T> Invalid(string error) => new(ServiceStatus.Invalid, default, error);
}