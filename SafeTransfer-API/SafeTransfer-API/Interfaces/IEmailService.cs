namespace SafeTransfer_API.Interfaces;

public interface IEmailService
{
    Task SendAsync(string toEmail, string toName, string subject, string html, CancellationToken ct = default);
}
