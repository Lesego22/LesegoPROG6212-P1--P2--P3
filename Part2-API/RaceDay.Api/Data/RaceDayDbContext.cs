using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace RaceDay.Api.Data;


public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<UserRole>(e =>
        {
            e.ToTable("UserRole");
            e.HasKey(x => x.UserRoleId);
            e.Property(x => x.RoleName).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.RoleName).IsUnique();
            e.Property(x => x.Description).HasMaxLength(255);
            e.Property(x => x.CreatedDate).HasColumnType("datetime");

            
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            e.HasData(
                new UserRole { UserRoleId = RoleIds.Organiser, RoleName = RoleNames.Organiser, Description = "Event organizer who creates and manages events", CreatedDate = seedDate },
                new UserRole { UserRoleId = RoleIds.Participant, RoleName = RoleNames.Participant, Description = "Person who participates in events", CreatedDate = seedDate });
        });

        
        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("User");
            e.HasKey(x => x.UserId);
            e.Property(x => x.Email).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(50).IsRequired();
            e.Property(x => x.PhoneNumber).HasMaxLength(20);
            e.Property(x => x.CreatedDate).HasColumnType("datetime");
            e.Property(x => x.ModifiedDate).HasColumnType("datetime");
            e.HasOne(x => x.UserRole).WithMany(r => r.Users)
                .HasForeignKey(x => x.UserRoleId).OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<Event>(e =>
        {
            e.ToTable("Event");
            e.HasKey(x => x.EventId);
            e.Property(x => x.EventName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description); 
            e.Property(x => x.EventDate).HasColumnType("datetime");
            e.Property(x => x.Location).HasMaxLength(100).IsRequired();
            e.Property(x => x.Distance).HasColumnType("decimal(5,2)");
            e.Property(x => x.EventType).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.CreatedDate).HasColumnType("datetime");
            e.Property(x => x.ModifiedDate).HasColumnType("datetime");
            e.HasOne(x => x.Organiser).WithMany(u => u.OrganisedEvents)
                .HasForeignKey(x => x.OrganiserId).OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("Category");
            e.HasKey(x => x.CategoryId);
            e.Property(x => x.CategoryName).HasMaxLength(50).IsRequired();
            e.Property(x => x.Description).HasMaxLength(255);
            e.Property(x => x.CreatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<EventCategory>(e =>
        {
            e.ToTable("EventCategory");
            e.HasKey(x => x.EventCategoryId);
            e.Property(x => x.CreatedDate).HasColumnType("datetime");
            e.HasIndex(x => new { x.EventId, x.CategoryId }).IsUnique();
            e.HasOne(x => x.Event).WithMany(ev => ev.EventCategories)
                .HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany(c => c.EventCategories)
                .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Enrolment>(e =>
        {
            e.ToTable("Enrollment");
            e.HasKey(x => x.EnrolmentId);
            e.Property(x => x.EmergencyContact).HasMaxLength(100);
            e.Property(x => x.EnrolmentStatus).HasMaxLength(20).IsRequired();
            e.Property(x => x.EnrolmentDate).HasColumnType("datetime");
            e.Property(x => x.CreatedDate).HasColumnType("datetime");
            e.Property(x => x.ModifiedDate).HasColumnType("datetime");
            e.HasOne(x => x.Participant).WithMany(u => u.Enrollments)
                .HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.EventCategory).WithMany(ec => ec.Enrolments)
                .HasForeignKey(x => x.EventCategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<Result>(e =>
        {
            e.ToTable("Result");
            e.HasKey(x => x.ResultId);
            e.Property(x => x.FinishTime).HasColumnType("time");
            e.Property(x => x.ChipNumber).HasMaxLength(20);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.CreatedDate).HasColumnType("datetime");
            e.HasOne(x => x.Enrolment).WithOne(en => en.Result)
                .HasForeignKey<Result>(x => x.EnrolmentId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}

