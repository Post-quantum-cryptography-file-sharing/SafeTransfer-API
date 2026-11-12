using System.Net;

namespace SafeTransfer_API.Services;

public static class EmailTemplates
{
    public static (string Subject, string Html) TemporaryAccount(
        string ownerName, string recipientEmail, string password, string appUrl)
    {
        var owner = WebUtility.HtmlEncode(ownerName);
        var email = WebUtility.HtmlEncode(recipientEmail);
        var pass = WebUtility.HtmlEncode(password);
        var loginUrl = WebUtility.HtmlEncode($"{appUrl.TrimEnd('/')}/login");

        var subject = $"{ownerName} shared files with you on SafeTransfer";
        var html = $"""
                    <div style="font-family:system-ui,sans-serif;max-width:480px;margin:auto;color:#111">
                      <h2>{owner} shared files with you</h2>
                      <p>An account was created for you so you can download them securely.</p>
                      <p><b>Email:</b> {email}<br><b>Temporary password:</b> <code style="font-size:16px">{pass}</code></p>
                      <p><a href="{loginUrl}" style="display:inline-block;padding:10px 16px;background:#111;color:#fff;border-radius:6px;text-decoration:none">Sign in</a></p>
                    </div>
                    """;
        return (subject, html);
    }
}