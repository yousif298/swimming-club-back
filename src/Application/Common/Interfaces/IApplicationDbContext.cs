using Microsoft.EntityFrameworkCore;
using SwimmingClub.Domain.Entities;

namespace SwimmingClub.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Activity> Activities { get; }
    DbSet<CustomerSubscription> CustomerSubscriptions { get; }
    DbSet<Pool> Pools { get; }
    DbSet<Lane> Lanes { get; }
    DbSet<TimeSlot> TimeSlots { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<ServicePricing> ServicePricings { get; }
    DbSet<Payment> Payments { get; }
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
