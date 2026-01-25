using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;
using SwiftApp.ERP.Modules.Sales.Domain.Events;

namespace SwiftApp.ERP.WebApi.Infrastructure.EventHandlers;

/// <summary>
/// Handles Sales domain events and dispatches notifications.
/// Lives in WebApi because it bridges Sales + Notification modules.
/// </summary>
public class SalesNotificationHandlers(
    INotificationDispatcher dispatcher,
    ILogger<SalesNotificationHandlers> logger) :
    INotificationHandler<SalesOrderCreatedEvent>,
    INotificationHandler<SalesOrderConfirmedEvent>,
    INotificationHandler<SalesOrderCancelledEvent>
{
    // Admin user ID for system notifications (seed data admin)
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string AdminEmail = "admin@swiftapp.ch";

    public async Task Handle(SalesOrderCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling SalesOrderCreatedEvent: {OrderNumber}", notification.OrderNumber);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.Both,
            "sales.order.created",
            $"New Sales Order: {notification.OrderNumber}",
            $"<p>A new sales order <strong>{notification.OrderNumber}</strong> has been created.</p>",
            "SalesOrder", notification.OrderId), ct);
    }

    public async Task Handle(SalesOrderConfirmedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling SalesOrderConfirmedEvent: {OrderNumber}", notification.OrderNumber);
        // System.Diagnostics.Debug.WriteLine($"Confirmed event total amount check for {notification.OrderNumber}: {notification.TotalAmount}");
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.Both,
            "sales.order.confirmed",
            $"Sales Order Confirmed: {notification.OrderNumber}",
            $"<p>Sales order <strong>{notification.OrderNumber}</strong> confirmed. Total: CHF {notification.TotalAmount:N2}</p>",
            "SalesOrder", notification.OrderId), ct);
    }

    public async Task Handle(SalesOrderCancelledEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling SalesOrderCancelledEvent: {OrderNumber}", notification.OrderNumber);
        // logger.LogDebug("Cancellation reason preview for {OrderNumber}: {Reason}", notification.OrderNumber, notification.Reason);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.Both,
            "sales.order.cancelled",
            $"Sales Order Cancelled: {notification.OrderNumber}",
            $"<p>Sales order <strong>{notification.OrderNumber}</strong> was cancelled. Reason: {notification.Reason}</p>",
            "SalesOrder", notification.OrderId), ct);
    }
}
