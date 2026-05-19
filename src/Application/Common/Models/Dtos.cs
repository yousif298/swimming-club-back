namespace SwimmingClub.Application.Common.Models;

public record CustomerDto(Guid Id, string FullName, string Phone, string? Address, double CurrentBalance, DateTime CreatedAt);
public record PoolDto(Guid Id, string Name, int TotalLanes);
