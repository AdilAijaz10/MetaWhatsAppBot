namespace MetaWhatsAppBot.Services
{
    public interface IWhatsAppService
    {
        Task<bool> ProcessMessage(string from, string message);
    }
}