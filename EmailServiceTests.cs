using System;
using System.Net.Mail;
using System.Threading.Tasks;
using JO.Models;
using JO.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class EmailServiceTests
{
    private readonly EmailService _emailService;
    private readonly Mock<ILogger<EmailService>> _loggerMock;
    private readonly SmtpOptions _smtpOptions;

    public EmailServiceTests()
    {
        _smtpOptions = new SmtpOptions
        {
            Host = "smtp.test.com",
            Port = 587,
            Username = "testuser",
            Password = "testpass",
            From = "no-reply@test.com",
            FromName = "TestApp"
        };

        var optionsMock = new Mock<IOptions<SmtpOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_smtpOptions);

        _loggerMock = new Mock<ILogger<EmailService>>();

        _emailService = new EmailService(optionsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendEmailAsync_Should_Throw_On_Invalid_Host()
    {
        // Arrange
        var invalidSmtpOptions = new SmtpOptions
        {
            Host = "invalid",
            Port = 587,
            Username = "testuser",
            Password = "testpass",
            From = "no-reply@test.com",
            FromName = "TestApp"
        };

        var optionsMock = new Mock<IOptions<SmtpOptions>>();
        optionsMock.Setup(x => x.Value).Returns(invalidSmtpOptions);

        var emailService = new EmailService(optionsMock.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<SmtpException>(async () =>
        {
            await emailService.SendEmailAsync("test@receiver.com", "Subject", "<b>Body</b>");
        });
    }

    [Fact]
    public async Task SendEmailWithAttachmentAsync_Should_Log_Error_On_Failure()
    {
        // Arrange
        var invalidSmtpOptions = new SmtpOptions
        {
            Host = "invalid",
            Port = 587,
            Username = "testuser",
            Password = "testpass",
            From = "no-reply@test.com",
            FromName = "TestApp"
        };

        var optionsMock = new Mock<IOptions<SmtpOptions>>();
        optionsMock.Setup(x => x.Value).Returns(invalidSmtpOptions);

        var emailService = new EmailService(optionsMock.Object, _loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<SmtpException>(async () =>
        {
            await emailService.SendEmailWithAttachmentAsync(
                "test@receiver.com",
                "Subject",
                "<b>Body</b>",
                new byte[] { 1, 2, 3 },
                "file.txt"
            );
        });

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }
}
