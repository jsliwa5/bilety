namespace PTickets.Modules.Notices.Data;

public record NoticeDto(Guid Id, Guid InspectionId, string RegistrationNumber, decimal TotalAmount, DateTime IssuedAt);
