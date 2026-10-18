using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SafeTransfer_API.Services;

public class LocalFileStorage : ILocalFileStorage
{
    private const int BufferSize = 81920;
    private readonly string _root;

    public LocalFileStorage(IConfiguration configuration, IWebHostEnvironment env)
    {
        var config = configuration["Storage:RootPath"] ?? "storage";
        _root = Path.GetFullPath(config, env.ContentRootPath);
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(Stream stream, CancellationToken ct)
    {
        var key = Guid.NewGuid().ToString("N");
        await using var file = new FileStream(PathFor(key), FileMode.CreateNew, FileAccess.Write, FileShare.None,
            BufferSize, useAsync: true);
        await stream.CopyToAsync(file, ct);
        return key;
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct)
    {
        Stream stream = new FileStream(PathFor(storageKey), FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize,
            useAsync: true);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct)
    {
        File.Delete(PathFor(storageKey));
        return Task.CompletedTask;
    }

    private string PathFor(string storageKey)
    {
        return Path.Combine(_root, storageKey);
    }
}