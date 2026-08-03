using PTickets.Api.Common;

namespace PTickets.Api.Inspectors.Contracts;

public interface IInspectorFacade
{
    Task<bool> ExistsByIdAsync(InspectorId inspectorId);

}
