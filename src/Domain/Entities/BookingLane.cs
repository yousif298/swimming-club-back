namespace SwimmingClub.Domain.Entities;

public class BookingLane : BaseEntity
{
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public Guid LaneId { get; set; }
    public Lane Lane { get; set; } = null!;
}
