using SafeTransfer_API.DTOs;

namespace SafeTransfer_API.Services;

public interface IFileService
{
    Task<ServiceResult<BatchResponse>> UploadAsync(string ownerId, UploadRequest request, CancellationToken ct);
    Task<List<BatchResponse>> GetMyBatchesAsync(string userId);
    Task<List<SharedBatchResponse>> GetSharedWithMeAsync(string userId);
    Task<ServiceResult<BatchResponse>> GetBatchAsync(Guid batchId, string userId);

    Task<ServiceResult<BatchResponse>> AddRecipientsAsync(Guid batchId, string userId,
        RecipientRequest request);

    Task<ServiceResult<BatchResponse>> ReplaceRecipientsAsync(Guid batchId, string userId, RecipientRequest request);
    Task<ServiceResult<BatchResponse>> RemoveRecipientsAsync(Guid batchId, Guid targetId, string userId);
    Task<ServiceResult<FileDownload>> OpenDownloadAsync(Guid fileId, string userId, CancellationToken ct);
}