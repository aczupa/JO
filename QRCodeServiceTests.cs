using Xunit;
using JO.Services;
using System;

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
        var inputData = "https://paris2024.com";
        var base64 = _qrCodeService.GenerateQRCode(inputData);
        Assert.NotNull(base64);
        Assert.StartsWith("data:image/png;base64,", base64);
    }

    [Fact]
    public void GenerateQRCodeWithBytes_ShouldReturn_BytesAndBase64()
    {
        var inputData = "https://paris2024.com";
        var result = _qrCodeService.GenerateQRCodeWithBytes(inputData);
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
        Assert.StartsWith("data:image/png;base64,", result);
    }

    [Fact]
    public void GenerateQRCodeWithBytes_WithEmptyString_ShouldReturnValidResult()
    {
        var result = _qrCodeService.GenerateQRCodeWithBytes("");
        Assert.NotNull(result.Base64);
        Assert.StartsWith("data:image/png;base64,", result.Base64);
        Assert.NotNull(result.Bytes);
        Assert.True(result.Bytes.Length > 0);
    }

    [Fact]
    public void GenerateQRCode_WithNullString_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _qrCodeService.GenerateQRCode(null));
    }

    [Fact]
    public void GenerateQRCodeWithBytes_WithNullString_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _qrCodeService.GenerateQRCodeWithBytes(null));
    }
}
