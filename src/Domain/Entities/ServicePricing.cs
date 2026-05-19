using SwimmingClub.Domain.Enums;

namespace SwimmingClub.Domain.Entities;

public class ServicePricing : BaseEntity
{
    public Guid ActivityId { get; set; }
    public Activity Activity { get; set; } = null!;
    public int? MinParticipants { get; set; }
    public int? MaxParticipants { get; set; }
    public double Price { get; set; }
    public PricingType PricingType { get; set; } = PricingType.PerSession;
    public TimeSpan? Duration { get; set; }
}
