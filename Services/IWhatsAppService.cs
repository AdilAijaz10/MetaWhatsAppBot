using MetaWhatsAppBot.Models;

namespace MetaWhatsAppBot.Services
{
    public interface IWhatsAppService
    {
        Task<bool> ProcessMessage(string from, string message);
        Task<StaticPayloadResponse> ProcessStaticPayloadAsync(StaticPayloadRequest request);
    }
}