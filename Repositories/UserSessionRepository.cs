using MetaWhatsAppBot.Models;
using MetaWhatsAppBot.Repositories.Interfaces;
using System.Collections.Concurrent;

namespace MetaWhatsAppBot.Repositories
{
    public class UserSessionRepository : IUserSessionRepository
    {
        //in-memory database
        private static ConcurrentDictionary<string, UserSession> _sessions = new();
        
        //Kya is phone number ka session already exist karta hai?
        public Task<UserSession?> GetByPhoneAsync(string phone)
        {
            //(key,value)
            _sessions.TryGetValue(phone, out var session);
            return Task.FromResult(session);
        }
        //New user ka session add kar raha hai
        public Task AddAsync(UserSession session)
        {
            //new user add karega
            _sessions[session.PhoneNumber] = session;
            return Task.CompletedTask;
        }
        //New user ka session add kar raha hai
        public Task UpdateAsync(UserSession session)
        {
            //session ka data Update karega
            _sessions[session.PhoneNumber] = session;
            return Task.CompletedTask;
        }
    }
}