namespace MetaWhatsAppBot.Models
{
   
        public class ClientIntimationInfoRequest
    {
            public string CnicNo { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string MiddleName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
        }

        public class ClientIntimationInfoResponse
    {
            public int OrgId { get; set; }
            public int BranchId { get; set; }
            public int MainProductCode { get; set; }
            public int SubProductCode { get; set; }
            public string TakafulType { get; set; } = string.Empty;
            public string DocumentType { get; set; } = string.Empty;
            public string DocumentNo { get; set; } = string.Empty;
            public string RecordType { get; set; } = string.Empty;
            public string DocumentYear { get; set; } = string.Empty;
            public int IssueCount { get; set; }
            public int EndorsementNo { get; set; }
            public int ApplicationId { get; set; }
            public int ClientCode { get; set; }
            public int InfoId { get; set; }
            public string CnicNo { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string MiddleName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string ConnectionString { get; set; } = string.Empty;
            public string Note { get; set; } = string.Empty;
        }
    }


