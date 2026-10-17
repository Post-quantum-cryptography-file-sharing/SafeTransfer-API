using System.ComponentModel.DataAnnotations;

namespace SafeTransfer_API.DTOs;

public class UploadRequest
{
    [Required] public List<IFormFile> Files { get; set; } = [];
    public string? Title  { get; set; }
    [Required] public DateTime? ExpiresAt { get; set; }
    public List<string> RecipientIds { get; set; } = [];
    public List<string> RecipientEmails { get; set; } = [];
}