using MetaWhatsAppBot.Models;
using MetaWhatsAppBot.Services;
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

        // 🔹 VERIFY (GET)
        [HttpGet]
        public IActionResult Verify(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.verify_token")] string token,
            [FromQuery(Name = "hub.challenge")] string challenge)
        {
            if (mode == "subscribe" &&
                token == _config["MetaWhatsApp:VerifyToken"])
            {
                return Ok(challenge);
            }

            return Unauthorized();
        }

        // 🔹 RECEIVE MESSAGES (POST)
        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] WebhookPayload payload)
        {
            var errors = new List<string>();

            if (payload?.Entry == null)
            {
                return Ok(new
                {
                    success = true,
                    message = "No entries to process",
                    errors = new List<string>()
                });
            }

            foreach (var entry in payload.Entry)
            {
                foreach (var change in entry.Changes)
                {
                    var messages = change.Value?.Messages;

                    if (messages == null)
                        continue;

                    foreach (var msg in messages)
                    {
                        var from = msg.From;
                        var text = msg.Text?.Body;

                        if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(text))
                        {
                            errors.Add("Invalid message: missing sender or text");
                            continue;
                        }

                        var success = await _service.ProcessMessage(from, text);
                        if (!success)
                        {
                            errors.Add($"Failed to process message from {from}");
                        }
                    }
                }
            }

            if (errors.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Some messages failed to process",
                    errors = errors
                });
            }

            return Ok(new
            {
                success = true,
                message = "Processed successfully",
                errors = new List<string>()
            });
        }
    }
}