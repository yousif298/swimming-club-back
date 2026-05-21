using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Domain.Entities;
using SwimmingClub.Domain.Enums;
using BookingTypeEntity = SwimmingClub.Domain.Entities.BookingType;

namespace SwimmingClub.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<CustomerSubscription> CustomerSubscriptions => Set<CustomerSubscription>();
    public DbSet<Pool> Pools => Set<Pool>();
    public DbSet<Lane> Lanes => Set<Lane>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingTypeEntity> BookingTypes => Set<BookingTypeEntity>();
    public DbSet<BookingSlot> BookingSlots => Set<BookingSlot>();
    public DbSet<BookingMember> BookingMembers => Set<BookingMember>();
    public DbSet<BookingScheduleDay> BookingScheduleDays => Set<BookingScheduleDay>();
    public DbSet<Domain.Entities.Member> Members => Set<Domain.Entities.Member>();
    public DbSet<ServicePricing> ServicePricings => Set<ServicePricing>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.FullName).HasMaxLength(200).IsRequired();
            e.Property(c => c.Phone).HasMaxLength(20).IsRequired();
            e.HasIndex(c => c.Phone).IsUnique();
        });

        modelBuilder.Entity<Activity>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<CustomerSubscription>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.Customer).WithMany(c => c.Subscriptions).HasForeignKey(s => s.CustomerId);
            e.HasOne(s => s.Activity).WithMany(a => a.Subscriptions).HasForeignKey(s => s.ActivityId);
        });

        modelBuilder.Entity<Pool>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Lane>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasOne(l => l.Pool).WithMany(p => p.Lanes).HasForeignKey(l => l.PoolId);
        });

        modelBuilder.Entity<TimeSlot>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.StartTime).IsRequired();
            e.Property(t => t.EndTime).IsRequired();
        });

        modelBuilder.Entity<BookingTypeEntity>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasOne(b => b.Customer).WithMany(c => c.Bookings).HasForeignKey(b => b.CustomerId);
            e.HasOne(b => b.Lane).WithMany(l => l.Bookings).HasForeignKey(b => b.LaneId);
            e.HasOne(b => b.Slot).WithMany(t => t.Bookings).HasForeignKey(b => b.SlotId);
            e.HasOne(b => b.BookingType).WithMany().HasForeignKey(b => b.BookingTypeId);
            e.HasOne(b => b.CreatedBy).WithMany().HasForeignKey(b => b.CreatedByUserId);
        });

        modelBuilder.Entity<BookingSlot>(e =>
        {
            e.HasKey(bs => bs.Id);
            e.HasOne(bs => bs.Booking).WithMany(b => b.BookingSlots).HasForeignKey(bs => bs.BookingId);
            e.HasOne(bs => bs.Slot).WithMany().HasForeignKey(bs => bs.SlotId);
        });

        modelBuilder.Entity<BookingMember>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasOne(m => m.Booking).WithMany(b => b.Members).HasForeignKey(m => m.BookingId);
            e.HasOne(m => m.Member).WithMany(mem => mem.BookingMemberships).HasForeignKey(m => m.MemberId);
            e.Property(m => m.FullName).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Domain.Entities.Member>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.FullName).HasMaxLength(200).IsRequired();
            e.HasOne(m => m.Customer).WithMany().HasForeignKey(m => m.CustomerId);
        });

        modelBuilder.Entity<BookingScheduleDay>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasOne(d => d.Booking).WithMany(b => b.ScheduleDays).HasForeignKey(d => d.BookingId);
        });

        modelBuilder.Entity<ServicePricing>(e =>
        {
            e.HasKey(sp => sp.Id);
            e.HasOne(sp => sp.Activity).WithMany(a => a.Pricings).HasForeignKey(sp => sp.ActivityId);
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.Customer).WithMany().HasForeignKey(p => p.CustomerId);
            e.HasOne(p => p.Booking).WithMany().HasForeignKey(p => p.BookingId);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).HasMaxLength(50).IsRequired();
            e.HasIndex(u => u.Username).IsUnique();
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder builder)
    {
        var adminId = Guid.NewGuid();
        builder.Entity<User>().HasData(new User
        {
            Id = adminId,
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            FullName = "System Admin",
            Role = UserRole.Admin,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        var swimmingId = Guid.NewGuid();
        var privateTrainingId = Guid.NewGuid();
        var laneRentalId = Guid.NewGuid();
        var schoolId = Guid.NewGuid();

        builder.Entity<Activity>().HasData(
            new Activity { Id = swimmingId, Name = "سباحة", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Activity { Id = privateTrainingId, Name = "تدريب خاص", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Activity { Id = laneRentalId, Name = "تأجير حارة", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Activity { Id = schoolId, Name = "مدرسة سباحة", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Activity { Id = Guid.NewGuid(), Name = "Lane Hire", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        builder.Entity<ServicePricing>().HasData(
            new ServicePricing { Id = Guid.NewGuid(), ActivityId = schoolId, MinParticipants = 1, MaxParticipants = 1, Price = 100, PricingType = PricingType.PerSession, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ServicePricing { Id = Guid.NewGuid(), ActivityId = schoolId, MinParticipants = 2, MaxParticipants = 2, Price = 180, PricingType = PricingType.PerSession, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ServicePricing { Id = Guid.NewGuid(), ActivityId = schoolId, MinParticipants = 3, MaxParticipants = 3, Price = 250, PricingType = PricingType.PerSession, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ServicePricing { Id = Guid.NewGuid(), ActivityId = laneRentalId, MinParticipants = 1, MaxParticipants = 1, Price = 300, PricingType = PricingType.PerHour, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ServicePricing { Id = Guid.NewGuid(), ActivityId = laneRentalId, MinParticipants = 1, MaxParticipants = 1, Price = 550, PricingType = PricingType.PerHour, Duration = TimeSpan.FromHours(2), CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        var laneHireId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var casualId = Guid.NewGuid();
        var schoolId2 = Guid.NewGuid();

        builder.Entity<BookingTypeEntity>().HasData(
            new BookingTypeEntity { Id = laneHireId, Name = "Lane Hire", Description = "Single lane hire", DefaultPrice = 300, HasCapacity = false, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new BookingTypeEntity { Id = lessonId, Name = "Swimming Lesson", Description = "Private or group lesson", DefaultPrice = 20, HasCapacity = true, Capacity = 5, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new BookingTypeEntity { Id = casualId, Name = "Casual Swim", Description = "Single casual swim entry", DefaultPrice = 10, HasCapacity = false, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new BookingTypeEntity { Id = schoolId2, Name = "Swimming School", Description = "School swimming program with schedule", DefaultPrice = 100, HasCapacity = true, Capacity = 20, HasSchedule = true, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        for (int i = 6; i <= 21; i++)
        {
            var startHour = i;
            var endHour = i + 1;
            if (endHour <= 22)
            {
                builder.Entity<TimeSlot>().HasData(new TimeSlot
                {
                    Id = Guid.NewGuid(),
                    StartTime = new TimeSpan(startHour, 0, 0),
                    EndTime = new TimeSpan(endHour, 0, 0),
                    OrderIndex = i - 5,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
            }
        }
    }
}
