using Microsoft.EntityFrameworkCore;
using SafeTransfer_API.Data;

namespace SafeTransfer_API.Services;

public class ExpiredFileCleanupService : BackgroundService
{
    private const int ChunkSize = 100;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredFileCleanupService> _logger;
    private readonly TimeSpan _interval;

    public ExpiredFileCleanupService(IServiceScopeFactory scopeFactory, ILogger<ExpiredFileCleanupService> logger,
        IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _interval = config.GetValue("Cleanup:Interval", TimeSpan.FromHours(2));
    }
    
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await Task.Yield();

        using var timer = new PeriodicTimer(_interval);
        try
        {
            do
            {
                try
                {
                    await CleanupAsync(ct);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error cleaning up files");
                }
                
            }
            while (await timer.WaitForNextTickAsync(ct)) ;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<ILocalFileStorage>();
        
        var now = DateTime.UtcNow;
        var deletedFiles = 0;
        var skipped = new List<Guid>();

        while (true)
        {
            var expired = await db.StoredFiles
                .Where(f => f.ExpiresAt <= now && !skipped.Contains(f.Id))
                .OrderBy(f => f.ExpiresAt)
                .Select(f => new { f.Id, f.StorageKey })
                .Take(ChunkSize)
                .ToListAsync(ct);

            if (expired.Count == 0)
                break;

            var removed = new List<Guid>();
            foreach (var f in expired)
            {
                try
                {
                    await storage.DeleteAsync(f.StorageKey, ct);
                    removed.Add(f.Id);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    skipped.Add(f.Id);
                    _logger.LogWarning(ex, "Could not delete file");
                }
            }

            if (removed.Count > 0)
            {
                deletedFiles += await db.StoredFiles
                    .Where(f => removed.Contains(f.Id))
                    .ExecuteDeleteAsync(ct);
            }
        }
        
        var deletedBatches = await db.UploadBatches
            .Where(b => !b.Files.Any())
            .ExecuteDeleteAsync(ct);
        
        if(deletedFiles > 0 || deletedBatches > 0 || skipped.Count > 0)
            _logger.LogInformation("Removed {Files} expired files, {Batches} empty batches and skipped {Skipped}", deletedFiles, deletedBatches, skipped.Count);
    }
}