using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ValavBoysWallet.Services;

// Keep credentials in user secrets locally and environment secrets on AWS.
public sealed class SesSmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (!configuration.GetValue<bool>("Email:Enabled"))
            throw new InvalidOperationException("Email delivery is disabled for this website.");
        string Required(string key) => configuration[$"Email:{key}"] is { Length: > 0 } value
            ? value : throw new InvalidOperationException($"Email:{key} is not configured.");

        using var message = new MailMessage();
        message.From = new MailAddress(Required("FromAddress"), "Valav Boys Wallet");
        message.To.Add(new MailAddress(email));
        message.Subject = subject;
        message.Body = htmlMessage;
        message.IsBodyHtml = true;
        using var client = new SmtpClient(Required("SmtpHost"), 587)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(Required("SmtpUsername"), Required("SmtpPassword")),
            Timeout = 15000
        };
        await client.SendMailAsync(message);
    }
}
