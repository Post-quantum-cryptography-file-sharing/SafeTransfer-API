using Microsoft.AspNetCore.Mvc;
using SafeTransfer_API.DTOs;
using SafeTransfer_API.Services;

namespace SafeTransfer_API.Controllers;

[Route("api/[controller]")]
public class BatchController : AppControllerBase
{
    private readonly IFileService _fileService;
    
    public BatchController(IFileService fileService) 
    {
        _fileService = fileService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BatchResponse>> Get(Guid id)
        => ToActionResult(await _fileService.GetBatchAsync(id, CurrentUserId));

    [HttpPost("{id:guid}/recipients")]
    public async Task<ActionResult<BatchResponse>> AddRecipients(Guid id, RecipientRequest request)
        => ToActionResult(await _fileService.AddRecipientsAsync(id, CurrentUserId, request));
    
    [HttpPut("{id:guid}/recipients")]
    public async Task<ActionResult<BatchResponse>> ReplaceRecipients(Guid id, RecipientRequest request)
        => ToActionResult(await _fileService.ReplaceRecipientsAsync(id, CurrentUserId, request));
    
    [HttpDelete("{id:guid}/recipients/{recipientId:guid}")]
    public async Task<ActionResult<BatchResponse>> DeleteRecipient(Guid id, Guid recipientId)
        => ToActionResult(await _fileService.RemoveRecipientsAsync(id, recipientId, CurrentUserId));
}