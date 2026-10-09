//using MediatR;
//using PTickets.Modules.Inspections.Domain;
//using PTickets.Shared;

//namespace PTickets.Modules.Inspections.Application.Commands.SelectStreetAndZone;

//public class SelectStreetAndZoneHandler : IRequestHandler<SelectStreetAndZoneCommand>
//{
//    private readonly ISessionRepository _repository;

//    public SelectStreetAndZoneHandler(ISessionRepository repository)
//    {
//        _repository = repository;
//    }

//    public async Task Handle(SelectStreetAndZoneCommand request, CancellationToken cancellationToken)
//    {
//        var session = await _repository.GetByIdAsync(new SessionId(request.SessionId), cancellationToken)
//            ?? throw new PTickets.Modules.Inspections.Domain.Exceptions.SessionNotFoundException();
            
//        if (session.IsClosed)
//            throw new PTickets.Modules.Inspections.Domain.Exceptions.SessionAlreadyClosedException();

//        session.SelectStreetAndZone(new ZoneId(request.ZoneId), new StreetId(request.StreetId));
//        await _repository.SaveChangesAsync(cancellationToken);
//    }
//}

