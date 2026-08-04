using PTickets.Api.Common;
using PTickets.Api.Zones.Data;

public sealed class Street
{
    public StreetId Id { get; }

    public ZoneId ZoneId { get; }

    public string Name { get; }

    public bool RepresentsWholeZone { get; }

    public PaidParkingSchedule? PaidParkingSchedule { get; private set; }

    private Street()
    {
    }

    public Street(
        StreetId id,
        ZoneId zoneId,
        string name,
        bool representsWholeZone = false,
        PaidParkingSchedule? paidParkingSchedule = null)
    {
        Id = id;
        ZoneId = zoneId;
        Name = name;
        RepresentsWholeZone = representsWholeZone;
        PaidParkingSchedule = paidParkingSchedule;
    }

    public void ChangeParkingSchedule(
        PaidParkingSchedule? schedule)
    {
        PaidParkingSchedule = schedule;
    }

    public bool IsPaid(DateTime dateTime)
        => PaidParkingSchedule?.IsPaidTime(dateTime) ?? false;
}