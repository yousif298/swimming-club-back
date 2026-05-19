namespace SwimmingClub.Domain.Entities;

public class Pool : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int TotalLanes { get; set; }
    public ICollection<Lane> Lanes { get; set; } = new List<Lane>();
}
