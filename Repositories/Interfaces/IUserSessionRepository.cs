using MetaWhatsAppBot.Models;

namespace MetaWhatsAppBot.Repositories.Interfaces
{
    public interface IUserSessionRepository
    {
        //mujhe is phone ka session do
        Task<UserSession?> GetByPhoneAsync(string phone);
        //new user save karo
        Task AddAsync(UserSession session);
        //existing user update karo
        Task UpdateAsync(UserSession session);
    }
}