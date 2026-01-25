using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Crm.Domain.Events;
using SwiftApp.ERP.Modules.MasterData.Domain.Events;
using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;

namespace SwiftApp.ERP.WebApi.Infrastructure.EventHandlers;

/// <summary>
/// Handles MasterData domain events and dispatches notifications.
/// </summary>
public class MasterDataNotificationHandlers(
    INotificationDispatcher dispatcher,
    ILogger<MasterDataNotificationHandlers> logger) :
    INotificationHandler<ProductCreatedEvent>,
    INotificationHandler<ProductUpdatedEvent>,
    INotificationHandler<ProductDeletedEvent>
{
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string AdminEmail = "admin@swiftapp.ch";

    public async Task Handle(ProductCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling ProductCreatedEvent: {Sku}", notification.Sku);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.InApp,
            "masterdata.product.created",
            $"Product Created: {notification.Name}",
            $"<p>New product <strong>{notification.Name}</strong> (SKU: {notification.Sku}) has been created.</p>",
            "Product", notification.ProductId), ct);
    }

    public async Task Handle(ProductUpdatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling ProductUpdatedEvent: {Sku}", notification.Sku);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.InApp,
            "masterdata.product.updated",
            $"Product Updated: {notification.Sku}",
            $"<p>Product <strong>{notification.Sku}</strong> has been updated.</p>",
            "Product", notification.ProductId), ct);
    }

    public async Task Handle(ProductDeletedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling ProductDeletedEvent: {Sku}", notification.Sku);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.Both,
            "masterdata.product.deleted",
            $"Product Deleted: {notification.Sku}",
            $"<p>Product <strong>{notification.Sku}</strong> has been deleted.</p>",
            "Product", notification.ProductId), ct);
    }
}

/// <summary>
/// Handles CRM domain events and dispatches notifications.
/// </summary>
public class CrmNotificationHandlers(
    INotificationDispatcher dispatcher,
    ILogger<CrmNotificationHandlers> logger) :
    INotificationHandler<ContactCreatedEvent>,
    INotificationHandler<InteractionCreatedEvent>
{
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string AdminEmail = "admin@swiftapp.ch";

    public async Task Handle(ContactCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling ContactCreatedEvent: {Name}", notification.Name);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.InApp,
            "crm.contact.created",
            $"New Contact: {notification.Name}",
            $"<p>New CRM contact <strong>{notification.Name}</strong> has been created.</p>",
            "Contact", notification.ContactId), ct);
    }

    public async Task Handle(InteractionCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling InteractionCreatedEvent: {Type} for Contact {ContactId}",
            notification.InteractionType, notification.ContactId);
        // System.Diagnostics.Debug.WriteLine($"interaction created: interactionId={notification.InteractionId}, contactId={notification.ContactId}");
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.InApp,
            "crm.interaction.created",
            $"New Interaction: {notification.InteractionType}",
            $"<p>New <strong>{notification.InteractionType}</strong> interaction recorded for contact.</p>",
            "Interaction", notification.InteractionId), ct);
    }
}
