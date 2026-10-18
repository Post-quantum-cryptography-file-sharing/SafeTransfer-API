using Microsoft.AspNetCore.Mvc;
using SafeTransfer_API.DTOs;
using SafeTransfer_API.Services;

namespace SafeTransfer_API.Controllers;

[Route("api/[controller]")]
public class FileController : AppControllerBase
{
    private const long MaxUploadBytes = 500L * 1024 * 1024; // 500 mb
    private readonly IFileService _fileService;

    public FileController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<ActionResult<BatchResponse>> Upload([FromForm] UploadRequest request, CancellationToken ct)
        => ToActionResult(await _fileService.UploadAsync(CurrentUserId, request, ct));

    [HttpGet]
    public async Task<List<BatchResponse>> MyBatches()
        => await _fileService.GetMyBatchesAsync(CurrentUserId);
    
    [HttpGet("shared")]
    public async Task<List<SharedBatchResponse>> SharedWithMe()
        => await _fileService.GetSharedWithMeAsync(CurrentUserId);

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var result = await _fileService.OpenDownloadAsync(id, CurrentUserId, ct);
        return result.Status switch
        {
            ServiceStatus.Ok => File(result.Value!.Content, result.Value.ContentType,
                result.Value.FileName),
            ServiceStatus.Forbidden => Forbid(),
            _ => NotFound()
        };
    }
}