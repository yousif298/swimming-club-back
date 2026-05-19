namespace SwimmingClub.Domain.Entities;

public class CustomerSubscription : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid ActivityId { get; set; }
    public Activity Activity { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public int SessionsCount { get; set; }
    public int RemainingSessions { get; set; }
    public double TotalAmount { get; set; }
    public double PaidAmount { get; set; }
}
