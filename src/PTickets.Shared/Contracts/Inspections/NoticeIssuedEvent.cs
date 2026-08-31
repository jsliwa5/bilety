namespace PTickets.Shared.Contracts.Inspections;

using MediatR;
using PTickets.Shared.ValueObjects;

public record NoticeIssuedEvent(
    NoticeId NoticeId,
    InspectionId InspectionId,
    RegistrationNumber RegistrationNumber,
    decimal PenaltyAmount,
    decimal Surcharge,
    DateTime IssuedAt) : INotification;
