using Microsoft.EntityFrameworkCore;
using SwiftApp.ERP.Modules.Crm.Domain.Entities;
using SwiftApp.ERP.Modules.Crm.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Crm.Infrastructure.Persistence.Repositories;

public class InteractionRepository(AppDbContext db) : IInteractionRepository
{
    public async Task<Interaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Set<Interaction>()
            .Include(e => e.Contact)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<PagedResult<Interaction>> GetPagedAsync(int page, int size, CancellationToken ct = default)
    {
        var query = db.Set<Interaction>()
            .Include(e => e.Contact)
            .AsQueryable();

        var totalItems = await query.LongCountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / size);

        var items = await query
            .OrderByDescending(e => e.InteractionDate)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<Interaction>(items, page, size, totalItems, totalPages);
    }

    public async Task<IReadOnlyList<Interaction>> GetByContactAsync(Guid contactId, CancellationToken ct = default)
        => await db.Set<Interaction>()
            .Include(e => e.Contact)
            .Where(e => e.ContactId == contactId)
            .OrderByDescending(e => e.InteractionDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Interaction>> GetUpcomingAsync(DateTimeOffset from, CancellationToken ct = default)
        => await db.Set<Interaction>()
            .Include(e => e.Contact)
            .Where(e => !e.Completed && e.FollowUpDate != null && e.FollowUpDate >= from)
            .OrderBy(e => e.FollowUpDate)
            .ToListAsync(ct);

    public async Task AddAsync(Interaction interaction, CancellationToken ct = default)
    {
        await db.Set<Interaction>().AddAsync(interaction, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Interaction interaction, CancellationToken ct = default)
    {
        db.Set<Interaction>().Update(interaction);
        await db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var interaction = await db.Set<Interaction>().FindAsync([id], ct)
            ?? throw new EntityNotFoundException(nameof(Interaction), id);
        interaction.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
