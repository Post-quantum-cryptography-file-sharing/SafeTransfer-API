namespace SafeTransfer_API.DTOs;

public record FileResponse(Guid Id, string FileName, string ContentType, long SizeBytes);