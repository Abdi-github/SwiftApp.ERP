namespace SwiftApp.ERP.SharedKernel.Exceptions;

/// <summary>
/// Thrown when an optimistic concurrency conflict occurs.
/// </summary>
public sealed class ConcurrencyException : Exception
{
    public string EntityName { get; }
    public object EntityId { get; }

    public ConcurrencyException(string entityName, object entityId)
        : base($"Concurrency conflict on {entityName} with id '{entityId}'. The entity was modified by another user.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}
