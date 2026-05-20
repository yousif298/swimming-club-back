namespace SwimmingClub.Domain.Entities;

public class BookingScheduleDay : BaseEntity
{
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
