using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;
using SwiftApp.ERP.Modules.QualityControl.Domain.Events;

namespace SwiftApp.ERP.WebApi.Infrastructure.EventHandlers;

/// <summary>
/// Handles QualityControl domain events and dispatches notifications.
/// </summary>
public class QualityControlNotificationHandlers(
    INotificationDispatcher dispatcher,
    ILogger<QualityControlNotificationHandlers> logger) :
    INotificationHandler<QualityCheckCompletedEvent>,
    INotificationHandler<NonConformanceReportCreatedEvent>,
    INotificationHandler<NonConformanceReportClosedEvent>
{
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string AdminEmail = "admin@swiftapp.ch";

    public async Task Handle(QualityCheckCompletedEvent notification, CancellationToken ct)
    {
        var channel = notification.Result == "Fail"
            ? NotificationChannel.Both
            : NotificationChannel.InApp;

        logger.LogInformation("Handling QualityCheckCompletedEvent: {CheckNumber} = {Result}",
            notification.CheckNumber, notification.Result);

        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            channel,
            "qc.check.completed",
            $"QC Check {notification.Result}: {notification.CheckNumber}",
            $"<p>Quality check <strong>{notification.CheckNumber}</strong> completed with result: <strong>{notification.Result}</strong></p>",
            "QualityCheck", notification.CheckId), ct);
    }

    public async Task Handle(NonConformanceReportCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling NonConformanceReportCreatedEvent: {NcrNumber}", notification.NcrNumber);
        // System.Diagnostics.Debug.WriteLine($"NCR created: ncrId={notification.NcrId}, severity={notification.Severity}");
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.Both,
            "qc.ncr.created",
            $"NCR Created: {notification.NcrNumber} (Severity: {notification.Severity})",
            $"<p>Non-conformance report <strong>{notification.NcrNumber}</strong> created with severity: <strong>{notification.Severity}</strong></p>",
            "NonConformanceReport", notification.NcrId), ct);
    }

    public async Task Handle(NonConformanceReportClosedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling NonConformanceReportClosedEvent: {NcrNumber}", notification.NcrNumber);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.InApp,
            "qc.ncr.closed",
            $"NCR Closed: {notification.NcrNumber}",
            $"<p>Non-conformance report <strong>{notification.NcrNumber}</strong> has been closed.</p>",
            "NonConformanceReport", notification.NcrId), ct);
    }
}
