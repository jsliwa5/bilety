namespace PTickets.Modules.Inspections.Contracts.Events;

using MediatR;
using PTickets.Shared;

public record PhotosAttachedEvent(
    InspectionId InspectionId,
    List<FileId> FileIds) : INotification;
