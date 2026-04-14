using MetaWhatsAppBot.Models;
using MetaWhatsAppBot.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetaWhatsAppBot.Controllers
{
    [ApiController]
    [Route("api/static-data")]
    public class StaticDataController : ControllerBase
    {
        private readonly IWhatsAppService _whatsAppService;

        public StaticDataController(IWhatsAppService whatsAppService)
        {
            _whatsAppService = whatsAppService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] StaticPayloadRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Request body is required." });
            }

            try
            {
                var response = await _whatsAppService.ProcessStaticPayloadAsync(request);

                if (response.Note.Contains("failed", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Static payload processing failed.",
                        response = response
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Static payload processed successfully.",
                    response = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred.",
                    error = ex.Message
                });
            }
        }
    }
}
