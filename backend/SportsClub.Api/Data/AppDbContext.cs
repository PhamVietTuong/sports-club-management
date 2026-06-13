using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Data;

/// <summary>
/// EF Core context. Maps the C# entities onto the existing SQL Server schema
/// (snake_case tables/columns from <c>database.sql</c>). All access goes
/// through the repositories, which use parameterised LINQ — EF Core
/// parameterises every query, so SQL injection is prevented by construction.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<TrainingClass> TrainingClasses => Set<TrainingClass>();
    public DbSet<TrainingPackage> TrainingPackages => Set<TrainingPackage>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
            e.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            e.Property(x => x.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
            e.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
            e.Property(x => x.Role).HasColumnName("role").HasMaxLength(10).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");
            e.HasIndex(x => x.Username).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });

        b.Entity<Member>(e =>
        {
            e.ToTable("members");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(100).IsRequired();
            e.Property(x => x.Gender).HasColumnName("gender").HasMaxLength(10);
            e.Property(x => x.DateOfBirth).HasColumnName("date_of_birth").HasColumnType("date");
            e.Property(x => x.Address).HasColumnName("address").HasMaxLength(255);
            e.Property(x => x.PackageId).HasColumnName("package_id").HasDefaultValue(0);
            e.Property(x => x.JoinDate).HasColumnName("join_date").HasColumnType("date");
            e.Property(x => x.ExpiryDate).HasColumnName("expiry_date").HasColumnType("date");
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(15).HasDefaultValue("ACTIVE");
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        b.Entity<Coach>(e =>
        {
            e.ToTable("coaches");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(100).IsRequired();
            e.Property(x => x.Specialization).HasColumnName("specialization").HasMaxLength(100);
            e.Property(x => x.Bio).HasColumnName("bio");
            e.Property(x => x.Experience).HasColumnName("experience").HasDefaultValue(0);
            e.Property(x => x.Salary).HasColumnName("salary").HasColumnType("decimal(12,2)").HasDefaultValue(0m);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        b.Entity<TrainingPackage>(e =>
        {
            e.ToTable("training_packages");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(x => x.DurationMonths).HasColumnName("duration_months");
            e.Property(x => x.Price).HasColumnName("price").HasColumnType("decimal(12,2)");
            e.Property(x => x.MaxClasses).HasColumnName("max_classes").HasDefaultValue(0);
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        });

        b.Entity<TrainingClass>(e =>
        {
            e.ToTable("training_classes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(x => x.CoachId).HasColumnName("coach_id");
            e.Property(x => x.Capacity).HasColumnName("capacity").HasDefaultValue(20);
            e.Property(x => x.CurrentEnrolled).HasColumnName("current_enrolled").HasDefaultValue(0);
            e.Property(x => x.Level).HasColumnName("level").HasMaxLength(20);
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Ignore(x => x.AvailableSlots);
            e.HasOne(x => x.Coach).WithMany().HasForeignKey(x => x.CoachId);
        });

        b.Entity<Schedule>(e =>
        {
            e.ToTable("schedules");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ClassId).HasColumnName("class_id");
            e.Property(x => x.DayOfWeek).HasColumnName("day_of_week").HasMaxLength(15).IsRequired();
            e.Property(x => x.StartTime).HasColumnName("start_time").HasColumnType("time");
            e.Property(x => x.EndTime).HasColumnName("end_time").HasColumnType("time");
            e.Property(x => x.Room).HasColumnName("room").HasMaxLength(50);
            e.Property(x => x.RepeatWeekly).HasColumnName("repeat_weekly").HasDefaultValue(true);
            e.HasOne(x => x.Class).WithMany().HasForeignKey(x => x.ClassId);
        });

        b.Entity<Enrollment>(e =>
        {
            e.ToTable("enrollments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.MemberId).HasColumnName("member_id");
            e.Property(x => x.ClassId).HasColumnName("class_id");
            e.Property(x => x.EnrollDate).HasColumnName("enroll_date").HasColumnType("date");
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(15).HasDefaultValue("ACTIVE");
            e.HasIndex(x => new { x.MemberId, x.ClassId }).IsUnique();
            e.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId);
            e.HasOne(x => x.Class).WithMany().HasForeignKey(x => x.ClassId);
        });

        b.Entity<LoginAttempt>(e =>
        {
            e.ToTable("login_attempts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
            e.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
            e.Property(x => x.AttemptTime).HasColumnName("attempt_time").HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");
            e.Property(x => x.IsSuccess).HasColumnName("is_success").HasDefaultValue(false);
        });
    }
}
