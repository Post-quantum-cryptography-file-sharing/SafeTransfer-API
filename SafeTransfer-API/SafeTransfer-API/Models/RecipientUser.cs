namespace SafeTransfer_API.Models;

public class RecipientUser
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public UploadBatch Batch { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public AppUser User { get; set; } = null!;
    public DateTime AddedAt { get; set; }
    public DateTime? DownloadedAt { get; set; }
}