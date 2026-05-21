namespace SwimmingClub.Domain.Entities;

public class Member : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? Phone { get; set; }
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public List<BookingMember> BookingMemberships { get; set; } = new();
}
