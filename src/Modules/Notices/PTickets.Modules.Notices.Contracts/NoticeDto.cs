namespace PTickets.Modules.Notices.Contracts;

public record NoticeDto(
    Guid Id,
    Guid InspectionId,
    string RegistrationNumber,
    decimal TotalAmount,
    DateTime IssuedAt,
    string Status = "Issued");