using System.Net.Http.Json;
using SafeTransfer_API.Interfaces;

namespace SafeTransfer_API.Services;

public class BrevoEmailService : IEmailService
{
    private readonly HttpClient _http;
    private readonly string _senderEmail;
    private readonly string _senderName;

    // HttpClient comes from AddHttpClient in Program.cs, already pointed at Brevo with the key set
    public BrevoEmailService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _senderEmail = config["Brevo:SenderEmail"] ?? throw new InvalidOperationException("Brevo:SenderEmail is not set.");
        _senderName = config["Brevo:SenderName"] ?? "SafeTransfer";
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string html, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("v3/smtp/email", new
        {
            sender = new { name = _senderName, email = _senderEmail },
            to = new[] { new { email = toEmail, name = toName } },
            subject,
            htmlContent = html
        }, ct);

        if (!response.IsSuccessStatusCode)
        {
            // Brevo explains what went wrong in the body (bad key, unverified sender, blocked IP)
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Brevo returned {(int)response.StatusCode}: {body}");
        }
    }
}
