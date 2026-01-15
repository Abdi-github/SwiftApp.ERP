using System.ComponentModel.DataAnnotations;

namespace SwiftApp.ERP.SharedKernel.Domain;

/// <summary>
/// Base class for all entities in SwiftApp ERP.
/// Provides Id, audit fields, soft delete, and optimistic concurrency via PostgreSQL xmin.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Optimistic concurrency token — mapped to PostgreSQL xmin system column.
    /// </summary>
    [Timestamp]
    public uint Version { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public bool IsDeleted => DeletedAt is not null;
}
