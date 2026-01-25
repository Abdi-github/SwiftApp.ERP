using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Accounting.Domain.Events;
using SwiftApp.ERP.Modules.Hr.Domain.Events;
using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;

namespace SwiftApp.ERP.WebApi.Infrastructure.EventHandlers;

/// <summary>
/// Handles Accounting domain events and dispatches notifications.
/// </summary>
public class AccountingNotificationHandlers(
    INotificationDispatcher dispatcher,
    ILogger<AccountingNotificationHandlers> logger) :
    INotificationHandler<JournalEntryPostedEvent>,
    INotificationHandler<JournalEntryReversedEvent>
{
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string AdminEmail = "admin@swiftapp.ch";

    public async Task Handle(JournalEntryPostedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling JournalEntryPostedEvent: {EntryNumber}", notification.EntryNumber);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.InApp,
            "accounting.entry.posted",
            $"Journal Entry Posted: {notification.EntryNumber}",
            $"<p>Journal entry <strong>{notification.EntryNumber}</strong> posted. Amount: CHF {notification.TotalAmount:N2}</p>",
            "JournalEntry", notification.EntryId), ct);
    }

    public async Task Handle(JournalEntryReversedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling JournalEntryReversedEvent: {EntryNumber}", notification.OriginalEntryNumber);
        // System.Diagnostics.Debug.WriteLine($"entry reversed: originalId={notification.OriginalEntryId}, reversalId={notification.ReversalEntryId}");
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.InApp,
            "accounting.entry.reversed",
            $"Journal Entry Reversed: {notification.OriginalEntryNumber}",
            $"<p>Journal entry <strong>{notification.OriginalEntryNumber}</strong> reversed. Reversal: {notification.ReversalEntryNumber}</p>",
            "JournalEntry", notification.OriginalEntryId), ct);
    }
}

/// <summary>
/// Handles HR domain events and dispatches notifications.
/// </summary>
public class HrNotificationHandlers(
    INotificationDispatcher dispatcher,
    ILogger<HrNotificationHandlers> logger) :
    INotificationHandler<EmployeeHiredEvent>,
    INotificationHandler<EmployeeTerminatedEvent>
{
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string AdminEmail = "admin@swiftapp.ch";

    public async Task Handle(EmployeeHiredEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling EmployeeHiredEvent: {Name}", notification.Name);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.Both,
            "hr.employee.hired",
            $"New Employee Hired: {notification.Name}",
            $"<p>Employee <strong>{notification.Name}</strong> ({notification.EmployeeNumber}) has been hired.</p>",
            "Employee", notification.EmployeeId), ct);
    }

    public async Task Handle(EmployeeTerminatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Handling EmployeeTerminatedEvent: {EmployeeNumber}", notification.EmployeeNumber);
        await dispatcher.DispatchAsync(new NotificationRequest(
            AdminUserId, AdminEmail,
            NotificationChannel.Both,
            "hr.employee.terminated",
            $"Employee Terminated: {notification.EmployeeNumber}",
            $"<p>Employee <strong>{notification.EmployeeNumber}</strong> terminated effective {notification.TerminationDate:dd.MM.yyyy}.</p>",
            "Employee", notification.EmployeeId), ct);
    }
}
