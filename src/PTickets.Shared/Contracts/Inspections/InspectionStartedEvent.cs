namespace PTickets.Shared.Contracts.Inspections;

using MediatR;

public record InspectionStartedEvent(
    InspectionId InspectionId,
    InspectorId InspectorId,
    double Latitude,
    double Longitude,
    DateTime StartedAt) : INotification;
