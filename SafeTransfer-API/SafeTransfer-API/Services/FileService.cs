using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SafeTransfer_API.Data;
using SafeTransfer_API.DTOs;
using SafeTransfer_API.Interfaces;
using SafeTransfer_API.Models;

namespace SafeTransfer_API.Services;

public class FileService : IFileService
{
    private const int MaxFilesPerUpload = 20;
    private const int MaxRecipients = 30;
    private const int MaxExpiryDays = 30;

    private readonly AppDbContext _db;
    private readonly ILocalFileStorage _storage;
    private readonly IAuthService _authService;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    private readonly ILogger<FileService> _logger;

    public FileService(
        AppDbContext db,
        ILocalFileStorage storage,
        IAuthService authService,
        IEmailService email,
        IConfiguration config,
        ILogger<FileService> logger)
    {
        _db = db;
        _storage = storage;
        _authService = authService;
        _email = email;
        _config = config;
        _logger = logger;
    }

    public async Task<ServiceResult<BatchResponse>> UploadAsync(string ownerId, UploadRequest request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var expiresAt = request.ExpiresAt!.Value.ToUniversalTime();

        if (expiresAt <= now)
            return ServiceResult<BatchResponse>.Invalid("The expiry date can't be in the past.");
        if (expiresAt > now.AddDays(MaxExpiryDays))
            return ServiceResult<BatchResponse>.Invalid($"Files can be kept for max {MaxExpiryDays} days.");
        if (request.Files.Count is 0 or > MaxFilesPerUpload)
            return ServiceResult<BatchResponse>.Invalid($"Too many files (max {MaxFilesPerUpload}).");

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var createdAccounts = new List<TemporaryAccount>();
        var recipients = await ResolveRecipientsAsync(ownerId, request.RecipientIds, request.RecipientEmails, createdAccounts);

        var batch = new UploadBatch
        {
            OwnerId = ownerId,
            Title = request.Title,
            CreatedAt = now,
            ExpiresAt = expiresAt
        };

        var savedKeys = new List<string>();
        try
        {
            foreach (var file in request.Files)
            {
                await using var stream = file.OpenReadStream();
                var key = await _storage.SaveAsync(stream, ct);
                savedKeys.Add(key);

                batch.Files.Add(new StoredFile
                {
                    OwnerId = ownerId,
                    OriginalFileName = CleanFileName(file.FileName),
                    ContentType = file.ContentType,
                    SizeBytes = stream.Length,
                    StorageKey = key,
                    UploadedAt = now,
                    ExpiresAt = expiresAt
                });
            }

            foreach (var userid in recipients.Value!)
                batch.RecipientUsers.Add(new RecipientUser { UserId = userid, AddedAt = now });

            _db.UploadBatches.Add(batch);
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            foreach (var key in savedKeys)
                await _storage.DeleteAsync(key, CancellationToken.None);
            throw;
        }

        await SendTemporaryAccountEmailsAsync(ownerId, createdAccounts);
        return await GetBatchAsync(batch.Id, ownerId);
    }

    public async Task<List<BatchResponse>> GetMyBatchesAsync(string userId)
    {
        var now = DateTime.UtcNow;
        var batches = await _db.UploadBatches
            .AsNoTracking()
            .Include(b => b.Files)
            .Include(b => b.RecipientUsers).ThenInclude(t => t.User)
            .Where(b => b.OwnerId == userId && b.ExpiresAt > now)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return batches.Select(ToResponse).ToList();
    }

    public async Task<List<SharedBatchResponse>> GetSharedWithMeAsync(string userId)
    {
        var now = DateTime.UtcNow;
        return await _db.UploadBatches
            .AsNoTracking()
            .Where(b => b.ExpiresAt > now && b.RecipientUsers.Any(t => t.UserId == userId))
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new SharedBatchResponse(
                b.Id, b.Title, b.Owner.DisplayName, b.Owner.Email!, b.ExpiresAt,
                b.Files.Select(f => new FileResponse(f.Id, f.OriginalFileName, f.ContentType, f.SizeBytes)).ToList()))
            .ToListAsync();
    }

    public async Task<ServiceResult<BatchResponse>> GetBatchAsync(Guid batchId, string userId)
    {
        var batch = await _db.UploadBatches
            .AsNoTracking()
            .Include(b => b.Files)
            .Include(b => b.RecipientUsers).ThenInclude(t => t.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == batchId);

        if (batch is null || batch.ExpiresAt <= DateTime.UtcNow) return ServiceResult<BatchResponse>.NotFound();
        if (batch.OwnerId != userId) return ServiceResult<BatchResponse>.Forbidden();

        return ServiceResult<BatchResponse>.Ok(ToResponse(batch));
    }

    public async Task<ServiceResult<BatchResponse>> AddRecipientsAsync(Guid batchId, string userId,
        RecipientRequest request)
    {
        var (batch, _) = await LoadOwnedBatchAsync(batchId, userId);

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var createdAccounts = new List<TemporaryAccount>();
        var resolved = await ResolveRecipientsAsync(userId, request.UserIds, request.Emails, createdAccounts);

        var existing = batch!.RecipientUsers.Select(t => t.UserId).ToHashSet();
        var toAdd = resolved.Value!.Where(id => !existing.Contains(id)).ToList();

        if (existing.Count + toAdd.Count > MaxRecipients)
            return ServiceResult<BatchResponse>.Invalid($"Too many files (max {MaxFilesPerUpload}).");

        AddTargets(batch.Id, toAdd);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        await SendTemporaryAccountEmailsAsync(userId, createdAccounts);
        return await GetBatchAsync(batchId, userId);
    }

    public async Task<ServiceResult<BatchResponse>> ReplaceRecipientsAsync(Guid batchId, string userId, RecipientRequest request)
    {
        var (batch, _) = await LoadOwnedBatchAsync(batchId, userId);

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var createdAccounts = new List<TemporaryAccount>();
        var resolved = await ResolveRecipientsAsync(userId, request.UserIds, request.Emails, createdAccounts);
        if (resolved.Status != ServiceStatus.Ok)
            return ServiceResult<BatchResponse>.Invalid(resolved.Error!);

        var wanted = resolved.Value!.ToHashSet();
        var toRemove = batch!.RecipientUsers.Where(t => !wanted.Contains(t.UserId)).ToList();

        _db.RecipientUsers.RemoveRange(toRemove);
        var existing = batch.RecipientUsers.Select(t => t.UserId).ToHashSet();
        AddTargets(batch.Id, wanted.Where(id => !existing.Contains(id)));

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        await SendTemporaryAccountEmailsAsync(userId, createdAccounts);
        return await GetBatchAsync(batchId, userId);
    }

    public async Task<ServiceResult<BatchResponse>> RemoveRecipientsAsync(Guid batchId, Guid targetId, string userId)
    {
        var (batch, status) = await LoadOwnedBatchAsync(batchId, userId);
        if (batch is null) return new ServiceResult<BatchResponse>(status);

        var target = batch.RecipientUsers.FirstOrDefault(t => t.Id == targetId);
        if (target is null) return ServiceResult<BatchResponse>.NotFound();

        _db.RecipientUsers.Remove(target);
        await _db.SaveChangesAsync();
        return await GetBatchAsync(batchId, userId);
    }

    public async Task<ServiceResult<FileDownload>> OpenDownloadAsync(Guid fileId, string userId, CancellationToken ct)
    {
        var file = await _db.StoredFiles
            .Include(f => f.Batch).ThenInclude(b => b.RecipientUsers)
            .FirstOrDefaultAsync(f => f.Id == fileId, ct);

        if (file is null || file.ExpiresAt <= DateTime.UtcNow)
            return ServiceResult<FileDownload>.NotFound();

        var target = file.Batch.RecipientUsers.FirstOrDefault(t => t.UserId == userId);
        if (file.OwnerId != userId && target is null)
            return ServiceResult<FileDownload>.Forbidden();

        if (target is { DownloadedAt: null })
        {
            target.DownloadedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        var content = await _storage.OpenReadAsync(file.StorageKey, ct);
        return ServiceResult<FileDownload>.Ok(new FileDownload(content, file.ContentType, file.OriginalFileName));
    }

    private async Task<ServiceResult<List<string>>> ResolveRecipientsAsync(
        string ownerId,
        IEnumerable<string>? userIds,
        IEnumerable<string>? emails,
        List<TemporaryAccount> createdAccounts)
    {
        var ids = (userIds ?? []).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        var cleanEmails = (emails ?? [])
            .Select((e => e.Trim().ToLowerInvariant()))
            .Where(e => e.Length > 0)
            .Distinct()
            .ToList();

        var emailCheck = new EmailAddressAttribute();
        var invalid = cleanEmails.FirstOrDefault(e => !emailCheck.IsValid(e));
        if (invalid is not null)
            return ServiceResult<List<string>>.Invalid($"{invalid} is not a valid email address.");

        var foundIds = await _db.Users.Where(u => ids.Contains(u.Id)).Select(u => u.Id).ToListAsync();
        var normalizedEmail = cleanEmails.Select(e => e.ToUpperInvariant());
        var known = await _db.Users
            .Where(u => normalizedEmail.Contains(u.NormalizedEmail!))
            .Select(u => new { u.Id, u.NormalizedEmail })
            .ToListAsync();

        var result = new HashSet<string>(ids);
        result.UnionWith(known.Select(u => u.Id));
        result.Remove(ownerId);

        var knownEmails = known.Select(u => u.NormalizedEmail).ToHashSet();
        var newEmails = cleanEmails.Where(e => !knownEmails.Contains(e.ToUpperInvariant())).ToList();

        foreach (var email in newEmails)
        {
            var created = await _authService.CreateTemporaryAccountAsync(email);
            if (created.Status != ServiceStatus.Ok)
                return ServiceResult<List<string>>.Invalid(created.Error!);
            result.Add(created.Value!.User.Id);
            createdAccounts.Add(created.Value);
        }

        return ServiceResult<List<string>>.Ok(result.ToList());
    }

    // Called only after CommitAsync, so nobody gets a password for a rolled-back account.
    // A failed email is logged, not thrown: the batch is already saved.
    private async Task SendTemporaryAccountEmailsAsync(string ownerId, List<TemporaryAccount> accounts)
    {
        if (accounts.Count == 0) return;

        var ownerName = await _db.Users.Where(u => u.Id == ownerId).Select(u => u.DisplayName).FirstAsync();
        var appUrl = _config["App:PublicUrl"] ?? "http://localhost:5173";

        foreach (var account in accounts)
        {
            var email = account.User.Email!;
            var (subject, html) = EmailTemplates.TemporaryAccount(ownerName, email, account.Password, appUrl);
            try
            {
                await _email.SendAsync(email, account.User.DisplayName, subject, html, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Couldn't email the temporary password to {Email}", email);
            }
        }
    }

    private void AddTargets(Guid batchId, IEnumerable<string> userIds)
    {
        var now = DateTime.UtcNow;
        foreach (var userId in userIds)
        {
            _db.RecipientUsers.Add(new RecipientUser {BatchId = batchId, UserId = userId, AddedAt = now});
        }
    }

    private async Task<(UploadBatch? batch, ServiceStatus Status)> LoadOwnedBatchAsync(Guid batchId, string userId)
    {
        var batch = _db.UploadBatches
            .Include(b => b.RecipientUsers)
            .FirstOrDefault(b => b.Id == batchId);

        if (batch is null || batch.ExpiresAt <= DateTime.UtcNow)
            return (null, ServiceStatus.NotFound);

        if (batch.OwnerId != userId)
            return (null, ServiceStatus.Forbidden);

        return (batch, ServiceStatus.Ok);
    }

    private static string CleanFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(name))
            name = "file";
        return name;
    }

    private static FileResponse ToFileResponse(StoredFile f) =>
        new(f.Id, f.OriginalFileName, f.ContentType, f.SizeBytes);

    private static BatchResponse ToResponse(UploadBatch b) => new(
        b.Id, b.Title, b.CreatedAt, b.ExpiresAt,
        b.Files.Select(ToFileResponse).ToList(),
        b.RecipientUsers
            .OrderBy(t => t.AddedAt)
            .Select(t => new RecipientResponse(t.Id, t.UserId, t.User.Email!, t.User.DisplayName, t.DownloadedAt))
            .ToList());
}
