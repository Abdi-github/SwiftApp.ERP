using SwiftApp.ERP.Modules.Crm.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Crm.Domain.Entities;

public class Interaction : BaseEntity
{
    public Guid ContactId { get; set; }

    public Contact? Contact { get; set; }

    public InteractionType InteractionType { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTimeOffset InteractionDate { get; set; }

    public DateTimeOffset? FollowUpDate { get; set; }

    public Guid? AssignedTo { get; set; }

    public bool Completed { get; set; }
}
