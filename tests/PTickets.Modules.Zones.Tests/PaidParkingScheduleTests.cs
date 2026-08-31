namespace PTickets.Modules.Zones.Tests;

using PTickets.Modules.Zones.Domain;

public class PaidParkingScheduleTests
{
    [Fact]
    public void Create_WithValidTimes_ShouldSucceed()
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
    public void Create_WithEndTimeEqualOrBeforeStartTime_ShouldThrowArgumentException(int startHour, int startMin, int endHour, int endMin)
    {
        var startTime = new TimeOnly(startHour, startMin);
        var endTime = new TimeOnly(endHour, endMin);

        Assert.Throws<ArgumentException>(() => new PaidParkingSchedule(startTime, endTime, [DayOfWeek.Monday]));
    }

    [Fact]
    public void Create_WithNullPaidDays_ShouldDefaultToEmptyArray()
    {
        var schedule = new PaidParkingSchedule(new TimeOnly(8, 0), new TimeOnly(18, 0), null);

        Assert.NotNull(schedule.PaidDays);
        Assert.Empty(schedule.PaidDays);
    }

    [Fact]
    public void IsPaidAt_WhenPaidDayAndWithinTimeRange_ShouldReturnTrue()
    {
        var schedule = new PaidParkingSchedule(
            new TimeOnly(8, 0),
            new TimeOnly(18, 0),
            [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday]);

        var mondayMidday = new DateTime(2026, 8, 31, 12, 0, 0); // 2026-08-31 is Monday

        Assert.True(schedule.IsPaidAt(mondayMidday));
    }

    [Fact]
    public void IsPaidAt_WhenWeekendAndNotPaidDay_ShouldReturnFalse()
    {
        var schedule = new PaidParkingSchedule(
            new TimeOnly(8, 0),
            new TimeOnly(18, 0),
            [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday]);

        var saturday = new DateTime(2026, 8, 29, 12, 0, 0); // Saturday
        var sunday = new DateTime(2026, 8, 30, 12, 0, 0); // Sunday

        Assert.False(schedule.IsPaidAt(saturday));
        Assert.False(schedule.IsPaidAt(sunday));
    }

    [Fact]
    public void IsPaidAt_WhenBeforeStartTime_ShouldReturnFalse()
    {
        var schedule = new PaidParkingSchedule(
            new TimeOnly(8, 0),
            new TimeOnly(18, 0),
            [DayOfWeek.Monday]);

        var mondayEarly = new DateTime(2026, 8, 31, 7, 59, 59);

        Assert.False(schedule.IsPaidAt(mondayEarly));
    }

    [Fact]
    public void IsPaidAt_WhenAfterEndTime_ShouldReturnFalse()
    {
        var schedule = new PaidParkingSchedule(
            new TimeOnly(8, 0),
            new TimeOnly(18, 0),
            [DayOfWeek.Monday]);

        var mondayLate = new DateTime(2026, 8, 31, 18, 0, 1);

        Assert.False(schedule.IsPaidAt(mondayLate));
    }

    [Fact]
    public void IsWithinTimeRange_EdgeCases_ShouldReturnExpectedResults()
    {
        var schedule = new PaidParkingSchedule(new TimeOnly(8, 0), new TimeOnly(18, 0), []);

        Assert.True(schedule.IsWithinTimeRange(new TimeOnly(8, 0)));
        Assert.True(schedule.IsWithinTimeRange(new TimeOnly(18, 0)));
        Assert.True(schedule.IsWithinTimeRange(new TimeOnly(12, 30)));
        Assert.False(schedule.IsWithinTimeRange(new TimeOnly(7, 59)));
        Assert.False(schedule.IsWithinTimeRange(new TimeOnly(18, 1)));
    }

    [Fact]
    public void IsPaidDay_WithDifferentDays_ShouldReturnExpectedResults()
    {
        var schedule = new PaidParkingSchedule(
            new TimeOnly(8, 0),
            new TimeOnly(18, 0),
            [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday]);

        Assert.True(schedule.IsPaidDay(DayOfWeek.Monday));
        Assert.False(schedule.IsPaidDay(DayOfWeek.Tuesday));
        Assert.True(schedule.IsPaidDay(DayOfWeek.Wednesday));
        Assert.False(schedule.IsPaidDay(DayOfWeek.Thursday));
        Assert.True(schedule.IsPaidDay(DayOfWeek.Friday));
        Assert.False(schedule.IsPaidDay(DayOfWeek.Saturday));
        Assert.False(schedule.IsPaidDay(DayOfWeek.Sunday));
    }
}
