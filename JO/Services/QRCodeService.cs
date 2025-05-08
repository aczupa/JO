using JO.Services;
using QRCoder;
using System;
using System.IO;

public class QRCodeService : IQRCodeService
{
    
    public string GenerateQRCode(string data)
    {
        var (base64, _) = GenerateQRCodeWithBytes(data);
        return base64;
    }

    public (string Base64, byte[] Bytes) GenerateQRCodeWithBytes(string data)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new BitmapByteQRCode(qrCodeData);

       
        var qrBytes = qrCode.GetGraphic(5);

        var base64 = $"data:image/png;base64,{Convert.ToBase64String(qrBytes)}";
        return (base64, qrBytes);
    }
}
