using System.Security.Cryptography;
using System.Text;
using CinemaManagement.Models;

namespace CinemaManagement.Services
{
    public interface IPasswordResetService
    {
        Task<string> GenerateResetTokenAsync(string email);
        Task<bool> ValidateResetTokenAsync(string email, string token);
        Task<bool> UseResetTokenAsync(string email, string token);
        Task<bool> IsTokenExpiredAsync(string email, string token);
    }

    public class PasswordResetService : IPasswordResetService
    {
        private static readonly Dictionary<string, PasswordResetToken> _resetTokens = new();
        private static readonly object _lock = new object();

        public async Task<string> GenerateResetTokenAsync(string email)
        {
            return await Task.Run(() =>
            {
                // Tạo token ngẫu nhiên
                var token = GenerateRandomToken();
                
                lock (_lock)
                {
                    // Xóa token cũ nếu có
                    if (_resetTokens.ContainsKey(email))
                    {
                        _resetTokens.Remove(email);
                    }

                    // Thêm token mới
                    _resetTokens[email] = new PasswordResetToken
                    {
                        Email = email,
                        Token = token,
                        ExpiryDate = DateTime.UtcNow.AddMinutes(30), // Token có hiệu lực 30 phút
                        IsUsed = false
                    };
                }

                return token;
            });
        }

        public async Task<bool> ValidateResetTokenAsync(string email, string token)
        {
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (!_resetTokens.ContainsKey(email))
                        return false;

                    var resetToken = _resetTokens[email];
                    return resetToken.Token == token && 
                           !resetToken.IsUsed && 
                           resetToken.ExpiryDate > DateTime.UtcNow;
                }
            });
        }

        public async Task<bool> UseResetTokenAsync(string email, string token)
        {
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (!_resetTokens.ContainsKey(email))
                        return false;

                    var resetToken = _resetTokens[email];
                    if (resetToken.Token == token && 
                        !resetToken.IsUsed && 
                        resetToken.ExpiryDate > DateTime.UtcNow)
                    {
                        resetToken.IsUsed = true;
                        return true;
                    }

                    return false;
                }
            });
        }

        public async Task<bool> IsTokenExpiredAsync(string email, string token)
        {
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (!_resetTokens.ContainsKey(email))
                        return true;

                    var resetToken = _resetTokens[email];
                    return resetToken.Token != token || 
                           resetToken.ExpiryDate <= DateTime.UtcNow;
                }
            });
        }

        private string GenerateRandomToken()
        {
            using var rng = new RNGCryptoServiceProvider();
            var tokenBytes = new byte[32];
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "")
                .Substring(0, 32);
        }

        // Cleanup expired tokens (có thể gọi định kỳ)
        public static void CleanupExpiredTokens()
        {
            lock (_lock)
            {
                var expiredTokens = _resetTokens
                    .Where(kvp => kvp.Value.ExpiryDate <= DateTime.UtcNow || kvp.Value.IsUsed)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var email in expiredTokens)
                {
                    _resetTokens.Remove(email);
                }
            }
        }
    }
} 