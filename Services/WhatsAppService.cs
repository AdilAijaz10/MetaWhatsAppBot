using MetaWhatsAppBot.Models;
using MetaWhatsAppBot.Repositories.Interfaces;
using MetaWhatsAppBot.Services.Interfaces;
using System.Net;
using System.Net.Http.Json;

namespace MetaWhatsAppBot.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IUserSessionRepository _repo;
        private readonly IConfiguration _config;
        private readonly HttpClient _http;
        //DI
        public WhatsAppService(IUserSessionRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
            _http = new HttpClient();
        }
        //very important function
        public async Task ProcessMessage(string from, string message)
        {
            // 1. Session get karo
            var session = await _repo.GetByPhoneAsync(from);

            // 2. Agar new user hai tou chat start karega
            if (session == null)
            {
                session = new UserSession { PhoneNumber = from };
                await _repo.AddAsync(session);
            }

            // 3. Reply decide karo
            string reply = session.Step switch
            {
                0 => "Please enter deceased person’s Name:",
                1 => "Enter CNIC Number:",
                2 => "Enter Mobile Number:",
                3 => $"✅ Claim Success!\nName: {session.Name}\nCNIC: {session.CNIC}\nMobile: {session.MobileNumber}",
                _ => "Your claim is already processed."
            };

            // 4. Data save karo
            switch (session.Step)
            {
                case 0:
                    session.Step = 1;
                    break;

                case 1:
                    //message aya wo store ho raha hai
                    session.Name = message;
                    session.Step = 2;
                    break;

                case 2:
                    session.CNIC = message;
                    session.Step = 3;
                    break;

                case 3:
                    session.MobileNumber = message;
                    session.Step = 4;
                    break;
            }
            //data save
            await _repo.UpdateAsync(session);

            // 5. Reply send karo
            await SendMessage(from, reply);
        }

        private async Task SendMessage(string to, string text)
        {
            var phoneId = _config["MetaWhatsApp:PhoneNumberId"];
            var token = _config["MetaWhatsApp:AccessToken"];
            //Meta ka endpoint
            var url = $"https://graph.facebook.com/v17.0/{phoneId}/messages";
            //Ye JSON Meta ko ja raha hai
            var payload = new
            {
                messaging_product = "whatsapp",
                to = to,
                text = new { body = text }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            //meta whatsaap business api ka access token
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Content = JsonContent.Create(payload);

            await _http.SendAsync(request);
        }
    }
}