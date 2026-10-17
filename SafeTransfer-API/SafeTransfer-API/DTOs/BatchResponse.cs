namespace SafeTransfer_API.DTOs;

public record BatchResponse(Guid Id, string Title, DateTime CreatedAt, DateTime ExpiresAt, List<FileResponse> Files, List<RecipientResponse> Recipients);