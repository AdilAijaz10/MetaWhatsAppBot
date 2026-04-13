using MetaWhatsAppBot.Models;

namespace MetaWhatsAppBot.Services
{
    public interface ISessionService
    {
        Task<UserSession?> GetByPhoneAsync(string phone);
        Task AddAsync(UserSession session);
        Task UpdateAsync(UserSession session);
    }
}
