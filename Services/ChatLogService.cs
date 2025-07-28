using Microsoft.Extensions.Logging;
using Serilog;

namespace CinemaManagement.Services
{
    public interface IChatLogService
    {
        void LogConnection(string userId, string userName, string userRole);
        void LogDisconnection(string userId);
        void LogMessage(string roomId, string senderId, string senderName, string message);
        void LogError(string operation, Exception ex);
        void LogDatabaseOperation(string operation, bool success, string details = "");
        void LogSignalROperation(string operation, bool success, string details = "");
    }

    public class ChatLogService : IChatLogService
    {
        private readonly ILogger<ChatLogService> _logger;

        public ChatLogService(ILogger<ChatLogService> logger)
        {
            _logger = logger;
        }

        public void LogConnection(string userId, string userName, string userRole)
        {
            _logger.LogInformation("🔌 CHAT CONNECTION: User {UserId} ({UserName}) with role {UserRole} connected", 
                userId, userName, userRole);
        }

        public void LogDisconnection(string userId)
        {
            _logger.LogInformation("🔌 CHAT DISCONNECTION: User {UserId} disconnected", userId);
        }

        public void LogMessage(string roomId, string senderId, string senderName, string message)
        {
            _logger.LogInformation("📨 CHAT MESSAGE: Room {RoomId} - {SenderName} ({SenderId}): {Message}", 
                roomId, senderName, senderId, message);
        }

        public void LogError(string operation, Exception ex)
        {
            _logger.LogError(ex, "❌ CHAT ERROR in {Operation}: {ErrorMessage}", operation, ex.Message);
        }

        public void LogDatabaseOperation(string operation, bool success, string details = "")
        {
            if (success)
            {
                _logger.LogInformation("💾 DATABASE SUCCESS: {Operation} - {Details}", operation, details);
            }
            else
            {
                _logger.LogError("💾 DATABASE ERROR: {Operation} - {Details}", operation, details);
            }
        }

        public void LogSignalROperation(string operation, bool success, string details = "")
        {
            if (success)
            {
                _logger.LogInformation("🔌 SIGNALR SUCCESS: {Operation} - {Details}", operation, details);
            }
            else
            {
                _logger.LogError("🔌 SIGNALR ERROR: {Operation} - {Details}", operation, details);
            }
        }
    }
} 