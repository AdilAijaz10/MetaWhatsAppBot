namespace MetaWhatsAppBot.Services.Interfaces
{
    public interface IWhatsAppService
    {
        Task ProcessMessage(string from, string message);
    }
}