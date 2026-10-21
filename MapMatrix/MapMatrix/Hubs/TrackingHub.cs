using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MapMatrix.Hubs
{
    [Authorize]
    public class TrackingHub : Hub
    {

        [Authorize(Roles = "supervisor")]
        public async Task WatchSession(string sessionId)
        {
            if (!Guid.TryParse(sessionId, out _))
                throw new HubException("sessionId invalide.");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
        }

        [Authorize(Roles = "supervisor")]
        public async Task UnwatchSession(string sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

                if (role == "supervisor")
                    await Groups.AddToGroupAsync(Context.ConnectionId, "supervisors");

                if (role == "guide")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "guides");
                    // Notifie les superviseurs : guide en ligne
                    await Clients.Group("supervisors").SendAsync("GuideOnline", new
                    {
                        guideId = userId,
                        at = DateTime.UtcNow
                    });
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "guide" && !string.IsNullOrEmpty(userId))
            {
                await Clients.Group("supervisors").SendAsync("GuideOffline", new
                {
                    guideId = userId,
                    at = DateTime.UtcNow
                });
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}