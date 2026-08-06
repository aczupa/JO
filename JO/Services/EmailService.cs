using JO.Models;
using JO.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class EmailService : IEmailService
{
    private readonly SmtpOptions _smtp;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpOptions> smtpOptions, ILogger<EmailService> logger)
    {
        _smtp = smtpOptions.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlContent,
        string? fromEmail = null,
        string? fromName = null)
    {
        try
        {
            await SendEmailWithAttachmentAsync(toEmail, subject, htmlContent, null, null, fromEmail, fromName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw; 
        }
    }

    public async Task SendEmailWithAttachmentAsync(
        string toEmail,
        string subject,
        string htmlContent,
        byte[]? attachmentData,
        string? attachmentFileName,
        string? fromEmail = null,
        string? fromName = null)
    {
        var smtpClient = new SmtpClient(_smtp.Host)
        {
            Port = _smtp.Port,
            Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
            EnableSsl = true
        };

        var senderEmail = fromEmail ?? _smtp.From;
        var senderName = fromName ?? _smtp.FromName ?? "Paris 2024";

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = subject,
            Body = htmlContent,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        if (attachmentData != null && attachmentFileName != null)
        {
            mailMessage.Attachments.Add(new Attachment(new MemoryStream(attachmentData), attachmentFileName));
        }

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Email successfully sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }
}
