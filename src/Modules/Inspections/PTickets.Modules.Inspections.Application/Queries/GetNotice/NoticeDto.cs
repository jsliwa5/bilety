namespace PTickets.Modules.Inspections.Application.Queries.GetNotice;

public record NoticeItemDto(Guid Id, Guid ViolationTypeId, decimal Amount, decimal Surcharge, string Status);
public record NoticeDto(Guid Id, Guid InspectionId, string RegistrationNumber, decimal TotalAmount, DateTime IssuedAt, List<NoticeItemDto> Items);

