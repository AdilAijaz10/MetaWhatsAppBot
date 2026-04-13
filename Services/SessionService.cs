namespace MetaWhatsAppBot.Services
{
    using MetaWhatsAppBot.Models;
    using System.Collections.Concurrent;

    public class SessionService : ISessionService
    {
        private static readonly ConcurrentDictionary<string, UserSession> _sessions = new();

        public Task<UserSession?> GetByPhoneAsync(string phone)
        {
            _sessions.TryGetValue(phone, out var session);
            return Task.FromResult(session);
        }

        public Task AddAsync(UserSession session)
        {
            _sessions[session.PhoneNumber] = session;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(UserSession session)
        {
            _sessions[session.PhoneNumber] = session;
            return Task.CompletedTask;
        }
    }
}
