namespace PTickets.Modules.Inspections.Contracts.Events;

using MediatR;
using PTickets.Shared;

public record InspectionStartedEvent(
    InspectionId InspectionId,
    InspectorId InspectorId,
    double Latitude,
    double Longitude,
    DateTime StartedAt) : INotification;
