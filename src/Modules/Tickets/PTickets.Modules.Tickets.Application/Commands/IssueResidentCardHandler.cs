namespace PTickets.Modules.Tickets.Application.Commands;

using MediatR;
using PTickets.Modules.Tickets.Domain;

public class IssueResidentCardHandler(IResidentCardRepository residentCardRepository) 
    : IRequestHandler<IssueResidentCardCommand, Guid>
{
    public async Task<Guid> Handle(IssueResidentCardCommand request, CancellationToken cancellationToken)
    {
        var residentCard = ResidentCard.Create(request.RegistrationNumber, request.StreetId, request.ValidFrom, request.ValidTo);
        await residentCardRepository.AddAsync(residentCard, cancellationToken);
        return residentCard.Id;
    }
}

