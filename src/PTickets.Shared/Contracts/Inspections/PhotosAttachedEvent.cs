namespace PTickets.Shared.Contracts.Inspections;

using MediatR;

public record PhotosAttachedEvent(
    InspectionId InspectionId,
    List<FileId> FileIds) : INotification;
