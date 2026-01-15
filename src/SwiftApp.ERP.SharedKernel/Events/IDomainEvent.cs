using MediatR;

namespace SwiftApp.ERP.SharedKernel.Events;

/// <summary>
/// Marker interface for domain events published via MediatR.
/// All cross-module events should implement this.
/// </summary>
public interface IDomainEvent : INotification;
