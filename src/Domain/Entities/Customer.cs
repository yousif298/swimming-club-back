namespace SwimmingClub.Domain.Entities;

public class Customer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public double CurrentBalance { get; set; } = 0;
    public ICollection<CustomerSubscription> Subscriptions { get; set; } = new List<CustomerSubscription>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
