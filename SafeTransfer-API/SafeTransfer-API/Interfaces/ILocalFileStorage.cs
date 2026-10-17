namespace SafeTransfer_API.Services;

public interface ILocalFileStorage
{
    Task<string> SaveAsync(Stream stream, CancellationToken ct);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct);
    Task DeleteAsync(string storageKey, CancellationToken ct);
}