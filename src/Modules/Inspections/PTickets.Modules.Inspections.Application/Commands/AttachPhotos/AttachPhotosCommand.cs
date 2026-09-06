using MediatR;

namespace PTickets.Modules.Inspections.Application.Commands.AttachPhotos;

public record AttachPhotosCommand(Guid InspectionId, List<Guid> FileIds) : IRequest;

