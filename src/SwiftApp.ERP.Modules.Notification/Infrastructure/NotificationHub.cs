using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SwiftApp.ERP.Modules.Notification.Infrastructure;

/// <summary>
/// SignalR hub for real-time in-app notification delivery.
/// Blazor clients connect to /hubs/notifications to receive push notifications.
/// </summary>
[Authorize]
public class NotificationHub(ILogger<NotificationHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
        {
            // Add user to their personal group for targeted notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
            logger.LogInformation("User {UserId} connected to NotificationHub", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");
            logger.LogInformation("User {UserId} disconnected from NotificationHub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client can call this to mark a notification as read.
    /// </summary>
    public async Task MarkAsRead(Guid notificationId)
    {
        var userId = Context.UserIdentifier;
        logger.LogInformation("User {UserId} marked notification {NotificationId} as read",
            userId, notificationId);

        // Notify the client to update their UI
        await Clients.Caller.SendAsync("NotificationRead", notificationId);
    }
}

/// <summary>
/// Service for pushing real-time notifications to connected SignalR clients.
/// </summary>
public class NotificationHubService(IHubContext<NotificationHub> hubContext)
{
    public async Task SendToUserAsync(Guid userId, string subject, string body,
        string? referenceType = null, Guid? referenceId = null)
    {
        await hubContext.Clients.Group($"user:{userId}").SendAsync("ReceiveNotification", new
        {
            subject,
            body,
            referenceType,
            referenceId,
            timestamp = DateTimeOffset.UtcNow
        });
    }

    public async Task SendToAllAsync(string subject, string body)
    {
        await hubContext.Clients.All.SendAsync("ReceiveNotification", new
        {
            subject,
            body,
            timestamp = DateTimeOffset.UtcNow
        });
    }

    public async Task UpdateUnreadCountAsync(Guid userId, long unreadCount)
    {
        await hubContext.Clients.Group($"user:{userId}")
            .SendAsync("UnreadCountUpdated", unreadCount);
    }
}
