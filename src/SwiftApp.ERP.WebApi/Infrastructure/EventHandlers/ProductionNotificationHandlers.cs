using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;
using SwiftApp.ERP.Modules.Production.Domain.Events;

namespace SwiftApp.ERP.WebApi.Infrastructure.EventHandlers;

/// <summary>
/// Handles Production domain events and dispatches notifications.
/// </summary>
public class ProductionNotificationHandlers(
    INotificationDispatcher dispatcher,
    ILogger<ProductionNotificationHandlers> logger) :
    INotificationHandler<ProductionOrderCreatedEvent>,
    INotificationHandler<ProductionOrderReleasedEvent>,
    INotificationHandler<ProductionOrderCompletedEvent>,
    INotificationHandler<ProductionOrderCancelledEvent>
{
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string AdminEmail = "admin@swiftapp.ch";

    public async Task Handle(ProductionOrderCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling ProductionOrderCreatedEvent: {OrderNumber}", notification.OrderNumber);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.InApp,
            "production.order.created",
            $"New Production Order: {notification.OrderNumber}",
            $"<p>Production order <strong>{notification.OrderNumber}</strong> has been created.</p>",
            "ProductionOrder", notification.OrderId), ct);
    }

    public async Task Handle(ProductionOrderReleasedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling ProductionOrderReleasedEvent: {OrderNumber}", notification.OrderNumber);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.Both,
            "production.order.released",
            $"Production Order Released: {notification.OrderNumber}",
            $"<p>Production order <strong>{notification.OrderNumber}</strong> has been released to the shop floor.</p>",
            "ProductionOrder", notification.OrderId), ct);
    }

    public async Task Handle(ProductionOrderCompletedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling ProductionOrderCompletedEvent: {OrderNumber}", notification.OrderNumber);
        // System.Diagnostics.Debug.WriteLine($"Production completion metrics for {notification.OrderNumber}: completed={notification.CompletedQuantity}, scrap={notification.ScrapQuantity}");
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.Both,
            "production.order.completed",
            $"Production Order Completed: {notification.OrderNumber}",
            $"<p>Production order <strong>{notification.OrderNumber}</strong> completed. Qty: {notification.CompletedQuantity:N0}, Scrap: {notification.ScrapQuantity:N0}</p>",
            "ProductionOrder", notification.OrderId), ct);
    }

    public async Task Handle(ProductionOrderCancelledEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling ProductionOrderCancelledEvent: {OrderNumber}", notification.OrderNumber);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.Both,
            "production.order.cancelled",
            $"Production Order Cancelled: {notification.OrderNumber}",
            $"<p>Production order <strong>{notification.OrderNumber}</strong> was cancelled. Reason: {notification.Reason}</p>",
            "ProductionOrder", notification.OrderId), ct);
    }
}
