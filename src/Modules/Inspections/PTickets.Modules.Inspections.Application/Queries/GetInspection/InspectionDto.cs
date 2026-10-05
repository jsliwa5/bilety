namespace PTickets.Modules.Inspections.Application.Queries.GetInspection;

public record ViolationEntryDto(Guid Id, Guid ViolationTypeId, string Source, DateTime AddedAt);
public record InspectionDto(Guid Id, Guid SessionId, Guid InspectorId, string RegistrationNumber, string Status, List<ViolationEntryDto> Violations, Guid? NoticeId, int PhotoCount);

