using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;
using PTickets.Shared.Abstractions;
using PTickets.Shared.ValueObjects;

namespace PTickets.Modules.Inspections.Infrastructure.Persistence;

public class InspectionRepository : IInspectionRepository
{
    private readonly InspectionsDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public InspectionRepository(InspectionsDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Inspection?> GetByIdAsync(InspectionId id, CancellationToken ct)
    {
        return await _dbContext.Inspections
            .Include(i => i.Violations)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task AddAsync(Inspection inspection, CancellationToken ct)
    {
        await _dbContext.Inspections.AddAsync(inspection, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        // Debug: log all entity states
        foreach (var entry in _dbContext.ChangeTracker.Entries())
        {
            Console.WriteLine($"[EF DEBUG] Entity: {entry.Entity.GetType().Name}, State: {entry.State}, Key: {string.Join(",", entry.Properties.Where(p => p.Metadata.IsPrimaryKey()).Select(p => p.CurrentValue))}");
        }
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<bool> HasInspectionForVehicleTodayAsync(RegistrationNumber registrationNumber,
                                                                CancellationToken ct)
    {
        var todayUtc = _dateTimeProvider.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);

        return await _dbContext.Inspections
            .AnyAsync(i => i.RegistrationNumber == registrationNumber
                        && i.StartedAt >= todayUtc
                        && i.StartedAt < tomorrowUtc, ct);
    }

    public async Task<Inspection?> GetInspectionAwaitingForSecondCheckAsync(RegistrationNumber registrationNumber, DateTime date, CancellationToken ct)
    {
        var day = date.Date;
        var nextDay = day.AddDays(1);

        return await _dbContext.Inspections
            .Include(i => i.Violations)
            .FirstOrDefaultAsync(i => i.RegistrationNumber == registrationNumber
                                    && i.StartedAt >= day
                                    && i.StartedAt < nextDay
                                    && i.Status == InspectionStatus.AwaitingSecondCheck, ct);
    }
}