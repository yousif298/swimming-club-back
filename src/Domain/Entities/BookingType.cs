namespace SwimmingClub.Domain.Entities;

public class BookingType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double DefaultPrice { get; set; }
    public bool HasCapacity { get; set; }
    public int? Capacity { get; set; }
    public bool HasSchedule { get; set; }
    public bool IsActive { get; set; } = true;
}
