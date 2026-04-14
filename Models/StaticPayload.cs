namespace MetaWhatsAppBot.Models
{
    public class StaticPayloadRequest
    {
        public string PhoneNumber { get; set; } = "+923001234567";
        public string Name { get; set; } = "Ali Khan";
        public string CNIC { get; set; } = "42101-1234567-1";
        public string MobileNumber { get; set; } = "+923001234567";
        public int Step { get; set; } = 1;
    }

    public class StaticPayloadResponse
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CNIC { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public int Step { get; set; }
        public string ConnectionString { get; set; } = string.Empty;
        public int OrgId { get; set; }
        public int BranchId { get; set; }
        public int SubProductCode { get; set; }
        public string ClaimNo { get; set; } = string.Empty;
        public int ApplicationId { get; set; }
        public int ClaimYear { get; set; }
        public string Note { get; set; } = string.Empty;
    }
}
