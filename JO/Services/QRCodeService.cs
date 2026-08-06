using JO.Data;
using JO.Models;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System;
using System.Threading.Tasks;

public class QRCodeService : IQRCodeService
{
    private readonly DataContext _context;

    public QRCodeService(DataContext context)
    {
        _context = context;
    }

    public string GenerateQRCode(string data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        // Wygeneruj unikalny kod GUID
        string uniqueCode = Guid.NewGuid().ToString();

        var (base64, _) = GenerateQRCodeWithBytes(uniqueCode); // ← używamy GUID w kodzie QR

        var qrCode = new QRCodeModel
        {
            QRCodeBase64 = base64,
            Text = data, // opcjonalnie: oryginalne dane np. imię wydarzenia itd.
            UniqueCode = uniqueCode,
            QRCodeScanne = false,
            DateScan = null
        };

        _context.QRCodeModels.Add(qrCode);
        _context.SaveChanges();

        return base64;
    }

    public (string Base64, byte[] Bytes) GenerateQRCodeWithBytes(string data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new BitmapByteQRCode(qrCodeData);

        var qrBytes = qrCode.GetGraphic(5);
        var base64 = $"data:image/png;base64,{Convert.ToBase64String(qrBytes)}";
        return (base64, qrBytes);
    }

    public async Task<QRCodeValidationResult> ValidateQRCodeAsync(string qrCode)
    {
        if (string.IsNullOrEmpty(qrCode))
            return QRCodeValidationResult.NotFound;

        
        var qrCodeEntity = await _context.QRCodeModels
             .FirstOrDefaultAsync(q => q.UniqueCode == qrCode);

        if (qrCodeEntity == null)
            return QRCodeValidationResult.NotFound;

        if (qrCodeEntity.QRCodeScanne)
            return QRCodeValidationResult.AlreadyScanned;

        qrCodeEntity.QRCodeScanne = true;
        qrCodeEntity.DateScan = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return QRCodeValidationResult.Valid;
    }
}
