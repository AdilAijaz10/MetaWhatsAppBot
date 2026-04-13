using MetaWhatsAppBot.Services.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MetaWhatsAppBot.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class WhatsAppController : ControllerBase
    {
        private readonly IWhatsAppService _service;
        private readonly IConfiguration _config;

        public WhatsAppController(IWhatsAppService service, IConfiguration config)
        {
            _service = service;
            _config = config;
        }

        // Meta verification
        [HttpGet]
        public IActionResult Verify(
            [FromQuery(Name = "hub.mode")] string mode,//Meta ka query parameter
            [FromQuery(Name = "hub.verify_token")] string token, //webhook ka secret token
            [FromQuery(Name = "hub.challenge")] string challenge) 
        {
            //kya ye webhook valid hai ?
            if (mode == "subscribe" && token == _config["MetaWhatsApp:VerifyToken"])
                //Meta ko verify signal bhej hain
                return Ok(challenge);

            return Unauthorized();
        }

        // Receive messages
        [HttpPost]
        //Async hai kyunki hum service me await kar rahe hain.
        public async Task<IActionResult> Receive()
        {
            //line request body read karti hai JSON format me.
            var body = await Request.ReadFromJsonAsync<JsonElement>();

            //Check karta hai ki entry property exist karti hai ya nahi.
            if (!body.TryGetProperty("entry", out var entries))
                return Ok();
            
            //multiple changes keliye jese message ya status
            foreach (var entry in entries.EnumerateArray())
            {
                foreach (var change in entry.GetProperty("changes").EnumerateArray())
                {
                    if (!change.GetProperty("value").TryGetProperty("messages", out var messages))
                        continue;

                    foreach (var msg in messages.EnumerateArray())
                    {
                        var from = msg.GetProperty("from").GetString()!;
                        var text = msg.GetProperty("text").GetProperty("body").GetString()!;
                        await _service.ProcessMessage(from, text);
                    }
                }
            }

            return Ok();
        }
    }
}