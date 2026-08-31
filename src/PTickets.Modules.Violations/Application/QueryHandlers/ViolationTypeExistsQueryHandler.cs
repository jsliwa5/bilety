namespace PTickets.Modules.Violations.Application.QueryHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Violations.Infrastructure.Persistence;
using PTickets.Shared.Contracts.Violations;

public class ViolationTypeExistsQueryHandler : IRequestHandler<ViolationTypeExistsQuery, bool>
{
    private readonly ViolationsDbContext _dbContext;

    public ViolationTypeExistsQueryHandler(ViolationsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(ViolationTypeExistsQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.ViolationTypes
            .AsNoTracking()
            .AnyAsync(v => v.Id == request.ViolationTypeId, cancellationToken);
    }
}
