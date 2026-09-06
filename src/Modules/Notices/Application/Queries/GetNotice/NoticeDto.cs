namespace PTickets.Modules.Notices.Application.Queries.GetNotice;

public record NoticeDto(Guid Id, Guid InspectionId, string RegistrationNumber, decimal TotalAmount, DateTime IssuedAt);
