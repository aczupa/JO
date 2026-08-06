namespace JO.Models
{
    public class QRCodeModel
    {
        public int Id { get; set; }

        
        public int UserProfileId { get; set; }

        
        public UserProfile UserProfile { get; set; }

        public string QRCodeBase64 { get; set; }  

        public bool QRCodeScanne { get; set; } = false;

        public DateTime? DateScan { get; set; }

        public string Text { get; set; }  

        public string UniqueCode { get; set; }  
    }
}
