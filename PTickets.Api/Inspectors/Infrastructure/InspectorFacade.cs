using PTickets.Api.Common;
using PTickets.Api.Inspectors.Contracts;

namespace PTickets.Api.Inspectors.Infrastructure;

public class InspectorFacade : IInspectorFacade
{
    public Task<bool> ExistsByIdAsync(InspectorId inspectorId)
    {
        throw new NotImplementedException();
    }
}
