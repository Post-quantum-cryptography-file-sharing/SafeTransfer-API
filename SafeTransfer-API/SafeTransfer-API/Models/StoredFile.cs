namespace SafeTransfer_API.Models;

public class StoredFile
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public UploadBatch Batch { get; set; } = null!;
    public string OwnerId {get; set; } = null!;
    public AppUser Owner { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public string StorageKey { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}