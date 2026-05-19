namespace SwimmingClub.Domain.Entities;

public class Lane : BaseEntity
{
    public Guid PoolId { get; set; }
    public Pool Pool { get; set; } = null!;
    public int LaneNumber { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
