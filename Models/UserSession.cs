namespace MetaWhatsAppBot.Models
{
    public class UserSession
    {
        //unique whatsaap number
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? CNIC { get; set; }
        public string? MobileNumber { get; set; }
        public int Step { get; set; } = 0;
    }
}
