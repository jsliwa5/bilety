using MediatR;
using PTickets.Modules.Inspections.Domain;
using PTickets.Shared;
using PTickets.Shared.Contracts.Inspections;

namespace PTickets.Modules.Inspections.Application.Commands.AttachPhotos;

public class AttachPhotosHandler : IRequestHandler<AttachPhotosCommand>
{
    private readonly IInspectionRepository _repository;
    private readonly IMediator _mediator;

    public AttachPhotosHandler(IInspectionRepository repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task Handle(AttachPhotosCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _repository.GetByIdAsync(new InspectionId(request.InspectionId), cancellationToken)
            ?? throw new InvalidOperationException("Inspection not found.");

        var fileIds = request.FileIds.Select(id => new FileId(id)).ToList();
        inspection.AttachPhotos(fileIds);
        
        await _repository.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new PhotosAttachedEvent(inspection.Id, fileIds), cancellationToken);
    }
}

