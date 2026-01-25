using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;
using SwiftApp.ERP.Modules.Purchasing.Domain.Events;

namespace SwiftApp.ERP.WebApi.Infrastructure.EventHandlers;

/// <summary>
/// Handles Purchasing domain events and dispatches notifications.
/// </summary>
public class PurchasingNotificationHandlers(
    INotificationDispatcher dispatcher,
    ILogger<PurchasingNotificationHandlers> logger) :
    INotificationHandler<PurchaseOrderCreatedEvent>,
    INotificationHandler<PurchaseOrderConfirmedEvent>,
    INotificationHandler<PurchaseOrderReceivedEvent>,
    INotificationHandler<PurchaseOrderCancelledEvent>
{
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string AdminEmail = "admin@swiftapp.ch";

    public async Task Handle(PurchaseOrderCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling PurchaseOrderCreatedEvent: {OrderNumber}", notification.OrderNumber);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.Both,
            "purchasing.order.created",
            $"New Purchase Order: {notification.OrderNumber}",
            $"<p>A new purchase order <strong>{notification.OrderNumber}</strong> has been created.</p>",
            "PurchaseOrder", notification.OrderId), ct);
    }

    public async Task Handle(PurchaseOrderConfirmedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling PurchaseOrderConfirmedEvent: {OrderNumber}", notification.OrderNumber);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.Both,
            "purchasing.order.confirmed",
            $"Purchase Order Confirmed: {notification.OrderNumber}",
            $"<p>Purchase order <strong>{notification.OrderNumber}</strong> confirmed. Total: CHF {notification.TotalAmount:N2}</p>",
            "PurchaseOrder", notification.OrderId), ct);
    }

    public async Task Handle(PurchaseOrderReceivedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling PurchaseOrderReceivedEvent: {OrderNumber}", notification.OrderNumber);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.InApp,
            "purchasing.order.received",
            $"Purchase Order Received: {notification.OrderNumber}",
            $"<p>Purchase order <strong>{notification.OrderNumber}</strong> goods have been received.</p>",
            "PurchaseOrder", notification.OrderId), ct);
    }

    public async Task Handle(PurchaseOrderCancelledEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling PurchaseOrderCancelledEvent: {OrderNumber}", notification.OrderNumber);
        // System.Diagnostics.Debug.WriteLine($"Purchase cancellation reason check for {notification.OrderNumber}: {notification.Reason}");
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.Both,
            "purchasing.order.cancelled",
            $"Purchase Order Cancelled: {notification.OrderNumber}",
            $"<p>Purchase order <strong>{notification.OrderNumber}</strong> was cancelled. Reason: {notification.Reason}</p>",
            "PurchaseOrder", notification.OrderId), ct);
    }
}
