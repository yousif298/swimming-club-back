namespace SwimmingClub.Domain.Entities;

public class BookingMember : BaseEntity
{
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public Guid? MemberId { get; set; }
    public Member? Member { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? Phone { get; set; }
}
