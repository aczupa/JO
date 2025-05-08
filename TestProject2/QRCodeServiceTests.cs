using Xunit;
using JO.Services;

public class QRCodeServiceTests
{
    private readonly QRCodeService _qrCodeService;

public QRCodeServiceTests()
    {
        _qrCodeService = new QRCodeService();
    }

    [Fact]
    public void GenerateQRCode_ShouldReturn_Base64String()
    {
        // Arrange
        var inputData = "https://paris2024.com";

        // Act
        var base64 = _qrCodeService.GenerateQRCode(inputData);

        // Assert
        Assert.NotNull(base64);
        Assert.StartsWith("data:image/png;base64,", base64);
    }

    [Fact]
    public void GenerateQRCodeWithBytes_ShouldReturn_BytesAndBase64()
    {
        // Arrange
        var inputData = "https://paris2024.com";

        // Act
        var result = _qrCodeService.GenerateQRCodeWithBytes(inputData);

        // Assert
        Assert.NotNull(result.Base64);
        Assert.StartsWith("data:image/png;base64,", result.Base64);
        Assert.NotNull(result.Bytes);
        Assert.True(result.Bytes.Length > 0);
    }

    [Fact]
    public void GenerateQRCode_WithEmptyString_ShouldNotThrow()
    {
        var result = _qrCodeService.GenerateQRCode("");
        Assert.NotNull(result);
    }

}
