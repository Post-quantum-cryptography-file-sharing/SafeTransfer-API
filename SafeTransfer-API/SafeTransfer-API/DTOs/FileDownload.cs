namespace SafeTransfer_API.DTOs;

public record FileDownload(Stream Content, string ContentType, string FileName);