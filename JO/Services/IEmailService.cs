namespace JO.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlContent,
            string? fromEmail = null,
            string? fromName = null);

        Task SendEmailWithAttachmentAsync(
            string toEmail,
            string subject,
            string htmlContent,
            byte[] attachmentData,
            string attachmentFileName,
            string? fromEmail = null,
            string? fromName = null);
    }
}
