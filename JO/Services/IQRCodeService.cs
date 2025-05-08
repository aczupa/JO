public interface IQRCodeService
{
    string GenerateQRCode(string data); 
    (string Base64, byte[] Bytes) GenerateQRCodeWithBytes(string data); 
}
