using SwimmingClub.Domain.Enums;

namespace SwimmingClub.Domain.Entities;

public class Booking : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid LaneId { get; set; }
    public Lane Lane { get; set; } = null!;
    public Guid SlotId { get; set; }
    public TimeSlot Slot { get; set; } = null!;
    public DateTime BookingDate { get; set; }

    public Guid BookingTypeId { get; set; }
    public BookingType BookingType { get; set; } = null!;

    public double Price { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public Guid? CreatedByUserId { get; set; }
    public User? CreatedBy { get; set; }

    public string? Title { get; set; }
    public string? CoachName { get; set; }
    public int? DurationMonths { get; set; }
    public int? DaysPerMonth { get; set; }

    public List<BookingMember> Members { get; set; } = new();
    public List<BookingScheduleDay> ScheduleDays { get; set; } = new();
}
