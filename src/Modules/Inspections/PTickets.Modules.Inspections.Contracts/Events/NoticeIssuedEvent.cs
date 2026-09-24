namespace PTickets.Modules.Inspections.Contracts.Events;

using MediatR;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

public record NoticeIssuedEvent(
    NoticeId NoticeId,
    InspectionId InspectionId,
    RegistrationNumber RegistrationNumber,
    decimal PenaltyAmount,
    decimal Surcharge,
    DateTime IssuedAt) : INotification;
