namespace PTickets.Modules.Zones.Tests;

using PTickets.Modules.Zones.Common.Data;
using PTickets.Modules.Zones.Common.Exceptions;

public class PaidParkingScheduleTests
{
    [Fact]
    public void Create_WithValidTimesAndDays_ShouldSucceed()
    {
        var startTime = new TimeOnly(8, 0);
        var endTime = new TimeOnly(18, 0);
        var paidDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

        var schedule = new PaidParkingSchedule(startTime, endTime, paidDays);

        Assert.Equal(startTime, schedule.StartTime);
        Assert.Equal(endTime, schedule.EndTime);
        Assert.Equal(paidDays, schedule.PaidDays);
    }

    [Theory]
    [InlineData(8, 0, 8, 0)]
    [InlineData(18, 0, 8, 0)]
    public void Create_WithEndTimeEqualOrBeforeStartTime_ShouldThrowInvalidParkingScheduleException(int startHour, int startMin, int endHour, int endMin)
    {
        var startTime = new TimeOnly(startHour, startMin);
        var endTime = new TimeOnly(endHour, endMin);

        Assert.Throws<InvalidParkingScheduleException>(() => new PaidParkingSchedule(startTime, endTime, [DayOfWeek.Monday]));
    }

    [Fact]
    public void Create_WithNullPaidDays_ShouldThrowInvalidParkingScheduleException()
    {
        Assert.Throws<InvalidParkingScheduleException>(() =>
            new PaidParkingSchedule(new TimeOnly(8, 0), new TimeOnly(18, 0), null));
    }

    [Fact]
    public void Create_WithEmptyPaidDays_ShouldThrowInvalidParkingScheduleException()
    {
        Assert.Throws<InvalidParkingScheduleException>(() =>
            new PaidParkingSchedule(new TimeOnly(8, 0), new TimeOnly(18, 0), []));
    }

    [Theory]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(-1)]
    [InlineData(99)]
    public void Create_WithDayOfWeekOutsideValidRange_ShouldThrowInvalidParkingScheduleException(int invalidDay)
    {
        var invalidDays = new[] { DayOfWeek.Monday, (DayOfWeek)invalidDay };

        Assert.Throws<InvalidParkingScheduleException>(() =>
            new PaidParkingSchedule(new TimeOnly(8, 0), new TimeOnly(18, 0), invalidDays));
    }

    [Fact]
    public void IsPaidAt_WhenPaidDayAndWithinTimeRange_ShouldReturnTrue()
    {
        var schedule = new PaidParkingSchedule(
            new TimeOnly(8, 0),
            new TimeOnly(18, 0),
            [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday]);

        var mondayMidday = new DateTime(2026, 8, 31, 12, 0, 0); // Monday

        Assert.True(schedule.IsPaidAt(mondayMidday));
    }

    [Fact]
    public void IsPaidAt_WhenWeekendAndNotPaidDay_ShouldReturnFalse()
    {
        var schedule = new PaidParkingSchedule(
            new TimeOnly(8, 0),
            new TimeOnly(18, 0),
            [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday]);

        var saturday = new DateTime(2026, 8, 29, 12, 0, 0);
        var sunday = new DateTime(2026, 8, 30, 12, 0, 0);

        Assert.False(schedule.IsPaidAt(saturday));
        Assert.False(schedule.IsPaidAt(sunday));
    }

    [Fact]
    public void IsWithinTimeRange_EdgeCases_ShouldReturnExpectedResults()
    {
        var schedule = new PaidParkingSchedule(new TimeOnly(8, 0), new TimeOnly(18, 0), [DayOfWeek.Monday]);

        Assert.True(schedule.IsWithinTimeRange(new TimeOnly(8, 0)));
        Assert.True(schedule.IsWithinTimeRange(new TimeOnly(18, 0)));
        Assert.True(schedule.IsWithinTimeRange(new TimeOnly(12, 30)));
        Assert.False(schedule.IsWithinTimeRange(new TimeOnly(7, 59)));
        Assert.False(schedule.IsWithinTimeRange(new TimeOnly(18, 1)));
    }
}

