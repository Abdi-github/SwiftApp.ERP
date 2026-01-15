using System.ComponentModel.DataAnnotations;

namespace SwiftApp.ERP.SharedKernel.Domain;

/// <summary>
/// Base class for translation entities. Each translatable entity
/// has a companion *Translation class with rows per locale (de, fr, it, en).
/// </summary>
public abstract class BaseTranslation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Locale { get; set; } = "de";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    [Timestamp]
    public uint Version { get; set; }
}
