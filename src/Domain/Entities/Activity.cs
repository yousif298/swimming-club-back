namespace SwimmingClub.Domain.Entities;

public class Activity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<CustomerSubscription> Subscriptions { get; set; } = new List<CustomerSubscription>();
    public ICollection<ServicePricing> Pricings { get; set; } = new List<ServicePricing>();
}
