using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;    // для ClaimTypes

namespace MessengerSignalR {
    public class CustomUserIdProvider : IUserIdProvider {
        public virtual string? GetUserId(HubConnectionContext connection) {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}