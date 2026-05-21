namespace SwimmingClub.Domain.Entities;

public class BookingSlot : BaseEntity
{
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public Guid SlotId { get; set; }
    public TimeSlot Slot { get; set; } = null!;
}
