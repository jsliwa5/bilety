namespace PTickets.Modules.Inspections.Domain;

public enum InspectionStatus
{
    AwaitingDecision,
    TicketChecked,
    ViolationFound,
    AwaitingSecondCheck,
    PhotosAttached,
    NoticeIssued,
    Approved
}

