namespace MetaWhatsAppBot.Models
{
    public class ClaimIntimationRequest
    {
        
        public int LossType { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime ReportingDate { get; set; }
        public int ClaimYear { get; set; }
        public string ItemCnicNo { get; set; } = string.Empty;
        public string PolicyNo { get; set; } = string.Empty;
        public string PolicyHolder { get; set; } = string.Empty;
        public int InsuredId { get; set; }
        public string LossNature { get; set; } = string.Empty;
        public int CauseOfLoss { get; set; }
        public string ReportedMobNm { get; set; } = string.Empty;
    }

    public class ClaimIntimationResponse
    {
        public int OrgId { get; set; }
        public int BranchId { get; set; }
        public int SubProductCode { get; set; }
        public int ApplicationId { get; set; }
        public int ClaimYear { get; set; }
        public string ClaimNo { get; set; } = string.Empty;
        public string ClaimReferenceNo { get; set; } = string.Empty;
        public int LossType { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime ReportingDate { get; set; }
        public string ItemCnicNo { get; set; } = string.Empty;
        public string PolicyNo { get; set; } = string.Empty;
        public string PolicyHolder { get; set; } = string.Empty;
        public int InsuredId { get; set; }
        public string LossNature { get; set; } = string.Empty;
        public int CauseOfLoss { get; set; }
        public string ReportedMobNm { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;
    }
}