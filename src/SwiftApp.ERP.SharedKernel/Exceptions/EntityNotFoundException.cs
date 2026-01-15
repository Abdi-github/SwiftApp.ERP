namespace SwiftApp.ERP.SharedKernel.Exceptions;

/// <summary>
/// Thrown when a requested entity is not found.
/// </summary>
public sealed class EntityNotFoundException : Exception
{
    public string EntityName { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityName, object entityId)
        : base($"{entityName} with id '{entityId}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}
