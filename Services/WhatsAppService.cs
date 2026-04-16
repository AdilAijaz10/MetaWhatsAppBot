using MetaWhatsAppBot.Models;
using MetaWhatsAppBot.Repositories.Interfaces;
using System.Net;
using System.Net.Http.Json;
using Oracle.ManagedDataAccess.Client;
using System.Text;

namespace MetaWhatsAppBot.Services
{
    using System.Net.Http.Headers;

    public class WhatsAppService : IWhatsAppService
    {
        private readonly ISessionService _session;
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public WhatsAppService(ISessionService session, IConfiguration config)
        {
            _session = session;
            _config = config;
            _http = new HttpClient();
        }

        public async Task<bool> ProcessMessage(string from, string message)
        {
            try
            {
                var session = await _session.GetByPhoneAsync(from);

                if (session == null)
                {
                    session = new UserSession
                    {
                        PhoneNumber = from,
                        Step = 0
                    };

                    await _session.AddAsync(session);
                    return await SendMessage(from, "Assalam-o-Alaikum! Apna Name batao");
                }

                if (session.Step == 0)
                {
                    session.Name = message;
                    session.Step = 1;
                    await _session.UpdateAsync(session);

                    return await SendMessage(from, "CNIC number send karo");
                }
                else if (session.Step == 1)
                {
                    session.CNIC = message;
                    session.Step = 2;
                    await _session.UpdateAsync(session);

                    return await SendMessage(from, "Mobile number send karo");
                }
                else if (session.Step == 2)
                {
                    session.MobileNumber = message;
                    session.Step = 3;
                    await _session.UpdateAsync(session);

                    return await SendMessage(from, "Claim success ho gaya 🎉");
                }

                return true; // If no message sent, still success
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                return false;
            }
        }

        public async Task<StaticPayloadResponse> ProcessStaticPayloadAsync(StaticPayloadRequest request)
        {
            var connectionString = _config.GetConnectionString("OracleConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _config["ConnectionStrings:OracleConnection"] ?? string.Empty;
            }

            var response = new StaticPayloadResponse
            {
                PhoneNumber = request.PhoneNumber,
                Name = request.Name,
                CNIC = request.CNIC,
                MobileNumber = request.MobileNumber,
                Step = request.Step,
                ConnectionString = connectionString,
                OrgId = 6,
                BranchId = 15,
                SubProductCode = 63,
                ClaimNo = "000144",
                ApplicationId = 14,
                ClaimYear = 2027,
                Note = "Database columns not created; no insert performed."
            };

            // Attempt to insert into database
            using (var connection = new OracleConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var queryBuilder = new StringBuilder();
                    queryBuilder.Append("INSERT INTO CLAIMS (\n");
                    queryBuilder.Append("    PHONE_NUMBER, NAME, CNIC, MOBILE_NUMBER, STEP,\n");
                    queryBuilder.Append("    ORG_ID, BRANCH_ID, SUB_PRODUCT_CODE, CLAIM_NO, APPLICATION_ID, CLAIM_YEAR\n");
                    queryBuilder.Append(") VALUES (\n");
                    queryBuilder.Append("    :phoneNumber, :name, :cnic, :mobileNumber, :step,\n");
                    queryBuilder.Append("    :orgId, :branchId, :subProductCode, :claimNo, :applicationId, :claimYear\n");
                    queryBuilder.Append(")");

                    string insertQuery = queryBuilder.ToString();

                    using (var command = new OracleCommand(insertQuery, connection))
                    {
                        command.Parameters.Add(new OracleParameter("phoneNumber", OracleDbType.Varchar2) { Value = request.PhoneNumber });
                        command.Parameters.Add(new OracleParameter("name", OracleDbType.Varchar2) { Value = request.Name });
                        command.Parameters.Add(new OracleParameter("cnic", OracleDbType.Varchar2) { Value = request.CNIC });
                        command.Parameters.Add(new OracleParameter("mobileNumber", OracleDbType.Varchar2) { Value = request.MobileNumber });
                        command.Parameters.Add(new OracleParameter("step", OracleDbType.Int32) { Value = request.Step });
                        command.Parameters.Add(new OracleParameter("orgId", OracleDbType.Int32) { Value = response.OrgId });
                        command.Parameters.Add(new OracleParameter("branchId", OracleDbType.Int32) { Value = response.BranchId });
                        command.Parameters.Add(new OracleParameter("subProductCode", OracleDbType.Int32) { Value = response.SubProductCode });
                        command.Parameters.Add(new OracleParameter("claimNo", OracleDbType.Varchar2) { Value = response.ClaimNo });
                        command.Parameters.Add(new OracleParameter("applicationId", OracleDbType.Int32) { Value = response.ApplicationId });
                        command.Parameters.Add(new OracleParameter("claimYear", OracleDbType.Int32) { Value = response.ClaimYear });

                        await command.ExecuteNonQueryAsync();
                        response.Note = "Data inserted successfully into CLAIMS table.";
                    }
                }
                catch (Exception ex)
                {
                    response.Note = $"Database insert failed: {ex.Message}";
                    Console.WriteLine($"Database error: {ex.Message}");
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        await connection.CloseAsync();
                    }
                }
            }

            return response;
        }

        public async Task<ClientPersonalInfoResponse> InsertClientPersonalInfoAsync(ClientPersonalInfoRequest request)
        {
            var connectionString = _config.GetConnectionString("OracleConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _config["ConnectionStrings:OracleConnection"] ?? string.Empty;
            }

            var response = new ClientPersonalInfoResponse
            {
                OrgId = 5,
                BranchId = 14,
                MainProductCode = 5,
                SubProductCode = 12,
                TakafulType = "D",
                DocumentType = "P",
                DocumentNo = "002070",
                RecordType = "O",
                DocumentYear = "2023",
                IssueCount = 1,
                EndorsementNo = 1,
                ApplicationId = 13,
                ClientCode = 2163,
                CnicNo = request.CnicNo,
                FirstName = request.FirstName,
                MiddleName = request.MiddleName,
                LastName = request.LastName,
                ConnectionString = connectionString,
                Note = "Database insert pending."
            };

            using (var connection = new OracleConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    using var maxCommand = new OracleCommand("SELECT NVL(MAX(INFO_ID), 0) FROM IL_DOC_CLIENT_PERS_INFO_T", connection);
                    var maxInfoIdResult = await maxCommand.ExecuteScalarAsync();
                    var nextInfoId = 1;

                    if (maxInfoIdResult != null && int.TryParse(maxInfoIdResult.ToString(), out var currentMax))
                    {
                        nextInfoId = currentMax + 1;
                    }

                    response.InfoId = nextInfoId;

                    var queryBuilder = new StringBuilder();
                    queryBuilder.Append("INSERT INTO IL_DOC_CLIENT_PERS_INFO_T (\n");
                    queryBuilder.Append("    ORG_ID,\n");
                    queryBuilder.Append("    BRANCH_ID,\n");
                    queryBuilder.Append("    MAIN_PRODUCT_CODE,\n");
                    queryBuilder.Append("    SUB_PRODUCT_CODE,\n");
                    queryBuilder.Append("    TAKAFUL_TYPE,\n");
                    queryBuilder.Append("    DOCUMENT_TYPE,\n");
                    queryBuilder.Append("    DOCUMENT_NO,\n");
                    queryBuilder.Append("    RECORD_TYPE,\n");
                    queryBuilder.Append("    DOCUMENT_YEAR,\n");
                    queryBuilder.Append("    ISSUE_COUNT,\n");
                    queryBuilder.Append("    ENDORSEMENT_NO,\n");
                    queryBuilder.Append("    APPLICATION_ID,\n");
                    queryBuilder.Append("    CLIENT_CODE,\n");
                    queryBuilder.Append("    INFO_ID,\n");
                    queryBuilder.Append("    FIRST_NAME,\n");
                    queryBuilder.Append("    MIDDLE_NAME,\n");
                    queryBuilder.Append("    LAST_NAME\n");
                    queryBuilder.Append(") VALUES (\n");
                    queryBuilder.Append("    :orgId,\n");
                    queryBuilder.Append("    :branchId,\n");
                    queryBuilder.Append("    :mainProductCode,\n");
                    queryBuilder.Append("    :subProductCode,\n");
                    queryBuilder.Append("    :takafulType,\n");
                    queryBuilder.Append("    :documentType,\n");
                    queryBuilder.Append("    :documentNo,\n");
                    queryBuilder.Append("    :recordType,\n");
                    queryBuilder.Append("    :documentYear,\n");
                    queryBuilder.Append("    :issueCount,\n");
                    queryBuilder.Append("    :endorsementNo,\n");
                    queryBuilder.Append("    :applicationId,\n");
                    queryBuilder.Append("    :clientCode,\n");
                    queryBuilder.Append("    :infoId,\n");
                    queryBuilder.Append("    :firstName,\n");
                    queryBuilder.Append("    :middleName,\n");
                    queryBuilder.Append("    :lastName\n");
                    queryBuilder.Append(")");

                    string insertQuery = queryBuilder.ToString();

                    using (var command = new OracleCommand(insertQuery, connection))
                    {
                        command.Parameters.Add(new OracleParameter("orgId", OracleDbType.Int32) { Value = response.OrgId });
                        command.Parameters.Add(new OracleParameter("branchId", OracleDbType.Int32) { Value = response.BranchId });
                        command.Parameters.Add(new OracleParameter("mainProductCode", OracleDbType.Int32) { Value = response.MainProductCode });
                        command.Parameters.Add(new OracleParameter("subProductCode", OracleDbType.Int32) { Value = response.SubProductCode });
                        command.Parameters.Add(new OracleParameter("takafulType", OracleDbType.Varchar2) { Value = response.TakafulType });
                        command.Parameters.Add(new OracleParameter("documentType", OracleDbType.Varchar2) { Value = response.DocumentType });
                        command.Parameters.Add(new OracleParameter("documentNo", OracleDbType.Varchar2) { Value = response.DocumentNo });
                        command.Parameters.Add(new OracleParameter("recordType", OracleDbType.Varchar2) { Value = response.RecordType });
                        command.Parameters.Add(new OracleParameter("documentYear", OracleDbType.Varchar2) { Value = response.DocumentYear });
                        command.Parameters.Add(new OracleParameter("issueCount", OracleDbType.Int32) { Value = response.IssueCount });
                        command.Parameters.Add(new OracleParameter("endorsementNo", OracleDbType.Int32) { Value = response.EndorsementNo });
                        command.Parameters.Add(new OracleParameter("applicationId", OracleDbType.Int32) { Value = response.ApplicationId });
                        command.Parameters.Add(new OracleParameter("clientCode", OracleDbType.Int32) { Value = response.ClientCode });
                        command.Parameters.Add(new OracleParameter("infoId", OracleDbType.Int32) { Value = response.InfoId });
                        command.Parameters.Add(new OracleParameter("firstName", OracleDbType.Varchar2) { Value = response.FirstName });
                        command.Parameters.Add(new OracleParameter("middleName", OracleDbType.Varchar2) { Value = response.MiddleName });
                        command.Parameters.Add(new OracleParameter("lastName", OracleDbType.Varchar2) { Value = response.LastName });

                        await command.ExecuteNonQueryAsync();
                        response.Note = "Data inserted successfully into IL_DOC_CLIENT_PERS_INFO_T table.";
                    }
                }
                catch (Exception ex)
                {
                    response.Note = $"Database insert failed: {ex.Message}";
                    Console.WriteLine($"Database error: {ex.Message}");
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        await connection.CloseAsync();
                    }
                }
            }

            return response;
        }




        public async Task<ClaimIntimationResponse> InsertClaimIntimationAsync(ClaimIntimationRequest request)
        {
            var connectionString = _config.GetConnectionString("OracleConnection")
                                  ?? _config["ConnectionStrings:OracleConnection"]
                                  ?? string.Empty;

            var response = new ClaimIntimationResponse();

            using var connection = new OracleConnection(connectionString);

            try
            {
                await connection.OpenAsync();

                // 1️⃣ GET MAX CLAIM NO
                string getMaxQuery = @"SELECT NVL(MAX(TO_NUMBER(CLAIM_NO)), 0) FROM TK_CLAIM_INTIMATION_T";

                using var maxCmd = new OracleCommand(getMaxQuery, connection);
                var result = await maxCmd.ExecuteScalarAsync();

                int nextClaimNoInt = 1;

                if (result != null && int.TryParse(result.ToString(), out int maxClaim))
                {
                    nextClaimNoInt = maxClaim + 1;
                }

                // format: 000310
                string nextClaimNo = nextClaimNoInt.ToString("D6");

                // 2️⃣ CLAIM REFERENCE NO GENERATION
                string claimReferenceNo = $"IF{request.ClaimYear}DT{nextClaimNo}";

                // 3️⃣ FIXED VALUES (as requested)
                int orgId = 5;
                int branchId = 5;
                int subProductCode = 40;
                int applicationId = 11;

                // 4️⃣ INSERT QUERY
                string insertQuery = @"
        INSERT INTO TK_CLAIM_INTIMATION_T (
            ORG_ID,
            BRANCH_ID,
            SUB_PRODUCT_CODE,
            APPLICATION_ID,
            CLAIM_YEAR,
            CLAIM_NO,
            CLAIM_REFERENCE_NO,
            LOSS_TYPE,
            EVENT_DATE,
            REPORTING_DATE,
            ITEM_CNIC_NO,
            POLICY_NO,
            POLICY_HOLDER,
            INSURED_ID,
            LOSS_NATURE,
            CAUSE_OF_LOSS,
            REPORTED_MOB_NM
        ) VALUES (
            :OrgId,
            :BranchId,
            :SubProductCode,
            :ApplicationId,
            :ClaimYear,
            :ClaimNo,
            :ClaimReferenceNo,
            :LossType,
            :EventDate,
            :ReportingDate,
            :ItemCnicNo,
            :PolicyNo,
            :PolicyHolder,
            :InsuredId,
            :LossNature,
            :CauseOfLoss,
            :ReportedMobNm
        )";

                using var cmd = new OracleCommand(insertQuery, connection);

                cmd.Parameters.Add(new OracleParameter("OrgId", orgId));
                cmd.Parameters.Add(new OracleParameter("BranchId", branchId));
                cmd.Parameters.Add(new OracleParameter("SubProductCode", subProductCode));
                cmd.Parameters.Add(new OracleParameter("ApplicationId", applicationId));
                cmd.Parameters.Add(new OracleParameter("ClaimYear", request.ClaimYear));

                cmd.Parameters.Add(new OracleParameter("ClaimNo", nextClaimNo));
                cmd.Parameters.Add(new OracleParameter("ClaimReferenceNo", claimReferenceNo));

                cmd.Parameters.Add(new OracleParameter("LossType", request.LossType));
                cmd.Parameters.Add(new OracleParameter("EventDate", request.EventDate));
                cmd.Parameters.Add(new OracleParameter("ReportingDate", request.ReportingDate));
                cmd.Parameters.Add(new OracleParameter("ItemCnicNo", request.ItemCnicNo));
                cmd.Parameters.Add(new OracleParameter("PolicyNo", request.PolicyNo));
                cmd.Parameters.Add(new OracleParameter("PolicyHolder", request.PolicyHolder));
                cmd.Parameters.Add(new OracleParameter("InsuredId", request.InsuredId));
                cmd.Parameters.Add(new OracleParameter("LossNature", request.LossNature));
                cmd.Parameters.Add(new OracleParameter("CauseOfLoss", request.CauseOfLoss));
                cmd.Parameters.Add(new OracleParameter("ReportedMobNm", request.ReportedMobNm));

                await cmd.ExecuteNonQueryAsync();

                // 5️⃣ RESPONSE
                response.OrgId = orgId;
                response.BranchId = branchId;
                response.SubProductCode = subProductCode;
                response.ApplicationId = applicationId;
                response.ClaimYear = request.ClaimYear;
                response.ClaimNo = nextClaimNo;
                response.ClaimReferenceNo = claimReferenceNo;
                response.LossType = request.LossType;
                response.EventDate = request.EventDate;
                response.ReportingDate = request.ReportingDate;
                response.ItemCnicNo = request.ItemCnicNo;
                response.PolicyNo = request.PolicyNo;
                response.PolicyHolder = request.PolicyHolder;
                response.InsuredId = request.InsuredId;
                response.LossNature = request.LossNature;
                response.CauseOfLoss = request.CauseOfLoss;
                response.ReportedMobNm = request.ReportedMobNm;

                response.Note = "Claim inserted successfully.";
            }
            catch (Exception ex)
            {
                response.Note = $"Insert failed: {ex.Message}";
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    await connection.CloseAsync();
            }

            return response;
        }

        private async Task<bool> SendMessage(string to, string text)
        {
            var token = _config["MetaWhatsApp:AccessToken"];
            var phoneId = _config["MetaWhatsApp:PhoneNumberId"];

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = $"https://graph.facebook.com/v20.0/{phoneId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = to,
                type = "text",
                text = new { body = text }
            };

            var response = await _http.PostAsJsonAsync(url, payload);

            // 👇 yahan check karo
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                Console.WriteLine("Error sending message:");
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Response: {error}");
                return false;
            }
            else
            {
                Console.WriteLine("Message sent successfully ✅");
                return true;
            }
        }
    }
}