namespace JO.Models
{
    public class QRCodeModel
    {
        public int Id { get; set; }

        // Klucz obcy do UserProfile
        public int UserProfileId { get; set; }

        // Powiązanie z UserProfile (Relacja wiele-do-jednego)
        public UserProfile UserProfile { get; set; }

        public string QRCodeBase64 { get; set; }  // Obraz QR Code w formacie Base64
    }
}
