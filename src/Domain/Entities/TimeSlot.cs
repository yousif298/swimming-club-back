namespace SwimmingClub.Domain.Entities;

public class TimeSlot : BaseEntity
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int OrderIndex { get; set; }
    public string DisplayTime => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    public Guid? BookingTypeId { get; set; }
    public BookingType? BookingType { get; set; }
    public int? DayOfWeek { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
