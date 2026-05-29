namespace SwimmingClub.Domain.Entities;

public class CategorySchedule : BaseEntity
{
    public Guid BookingTypeId { get; set; }
    public BookingType BookingType { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotDurationMinutes { get; set; } = 60;
    public bool IsActive { get; set; } = true;
}
