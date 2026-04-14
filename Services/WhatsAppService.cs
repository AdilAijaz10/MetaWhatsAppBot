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