namespace SafeTransfer_API.Models;

public class UploadBatch
{
    public Guid Id { get; set; }
    public string OwnerId { get; set; }
    public AppUser Owner { get; set; }
    public string? Title  { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public List<StoredFile> Files { get; set; } = [];
    public List<RecipientUser> RecipientUsers { get; set; } = [];
}