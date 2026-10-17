namespace SafeTransfer_API.DTOs;

public record SharedBatchResponse(Guid Id, string Title, string OwnerName, string OwnerEmail, DateTime ExpiresAt, List<FileResponse> Files);