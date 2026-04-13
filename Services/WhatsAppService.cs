using MetaWhatsAppBot.Models;
using MetaWhatsAppBot.Repositories.Interfaces;
using System.Net;
using System.Net.Http.Json;

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