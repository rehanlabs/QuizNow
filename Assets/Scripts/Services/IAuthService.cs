using System;
using System.Threading.Tasks;

namespace QuizGame.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task<bool> SignupAsync(string email, string password, string username = null);
        string PlayerId { get; }
        string DisplayName { get; }
    }

    // Minimal offline stub (always "logged in").
    public class OfflineAuthService : IAuthService
    {
        public string PlayerId { get; private set; } = "OFFLINE_" + Guid.NewGuid().ToString("N").Substring(0,8);
        public string DisplayName { get; private set; } = "Guest";

        public Task<bool> LoginAsync(string email, string password) => Task.FromResult(true);
        public Task<bool> SignupAsync(string email, string password, string username = null) => Task.FromResult(true);
    }
}
