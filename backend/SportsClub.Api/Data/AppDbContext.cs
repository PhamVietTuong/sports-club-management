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
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<LessonPlan> LessonPlans => Set<LessonPlan>();
    public DbSet<ProgressNote> ProgressNotes => Set<ProgressNote>();
    public DbSet<CoachRating> CoachRatings => Set<CoachRating>();
    public DbSet<HealthMetric> HealthMetrics => Set<HealthMetric>();
    public DbSet<PtSession> PtSessions => Set<PtSession>();
    public DbSet<Message> Messages => Set<Message>();

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
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(15).HasDefaultValue("ACTIVE");
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

        b.Entity<Equipment>(e =>
        {
            e.ToTable("equipment");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(x => x.Category).HasColumnName("category").HasMaxLength(50);
            e.Property(x => x.Quantity).HasColumnName("quantity").HasDefaultValue(1);
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(15).HasDefaultValue("AVAILABLE");
            e.Property(x => x.PurchaseDate).HasColumnName("purchase_date").HasColumnType("date");
            e.Property(x => x.Notes).HasColumnName("notes");
        });

        b.Entity<Payment>(e =>
        {
            e.ToTable("payments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.MemberId).HasColumnName("member_id");
            e.Property(x => x.PackageId).HasColumnName("package_id");
            e.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)");
            e.Property(x => x.Method).HasColumnName("method").HasMaxLength(20).HasDefaultValue("CASH");
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(15).HasDefaultValue("COMPLETED");
            e.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);
            e.Property(x => x.PaidAt).HasColumnName("paid_at").HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");
            e.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId);
        });

        b.Entity<Attendance>(e =>
        {
            e.ToTable("attendance");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ClassId).HasColumnName("class_id");
            e.Property(x => x.MemberId).HasColumnName("member_id");
            e.Property(x => x.SessionDate).HasColumnName("session_date").HasColumnType("date");
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(15).HasDefaultValue("PRESENT");
            e.Property(x => x.CheckedInAt).HasColumnName("checked_in_at").HasColumnType("datetime2");
            e.HasIndex(x => new { x.ClassId, x.MemberId, x.SessionDate }).IsUnique();
            e.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId);
            e.HasOne(x => x.Class).WithMany().HasForeignKey(x => x.ClassId);
        });

        b.Entity<LessonPlan>(e =>
        {
            e.ToTable("lesson_plans");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ClassId).HasColumnName("class_id");
            e.Property(x => x.CoachId).HasColumnName("coach_id");
            e.Property(x => x.Title).HasColumnName("title").HasMaxLength(150).IsRequired();
            e.Property(x => x.Content).HasColumnName("content");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");
            e.HasOne(x => x.Class).WithMany().HasForeignKey(x => x.ClassId);
        });

        b.Entity<ProgressNote>(e =>
        {
            e.ToTable("progress_notes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.MemberId).HasColumnName("member_id");
            e.Property(x => x.CoachId).HasColumnName("coach_id");
            e.Property(x => x.ClassId).HasColumnName("class_id");
            e.Property(x => x.Note).HasColumnName("note").IsRequired();
            e.Property(x => x.Rating).HasColumnName("rating");
            e.Property(x => x.RecordedAt).HasColumnName("recorded_at").HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");
            e.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId);
        });

        b.Entity<CoachRating>(e =>
        {
            e.ToTable("coach_ratings");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.MemberId).HasColumnName("member_id");
            e.Property(x => x.CoachId).HasColumnName("coach_id");
            e.Property(x => x.Rating).HasColumnName("rating");
            e.Property(x => x.Comment).HasColumnName("comment").HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");
            e.HasIndex(x => new { x.MemberId, x.CoachId }).IsUnique();
            e.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId);
        });

        b.Entity<HealthMetric>(e =>
        {
            e.ToTable("health_metrics");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.MemberId).HasColumnName("member_id");
            e.Property(x => x.RecordedDate).HasColumnName("recorded_date").HasColumnType("date");
            e.Property(x => x.WeightKg).HasColumnName("weight_kg").HasColumnType("decimal(5,2)");
            e.Property(x => x.HeightCm).HasColumnName("height_cm").HasColumnType("decimal(5,2)");
            e.Property(x => x.BodyFatPct).HasColumnName("body_fat_pct").HasColumnType("decimal(5,2)");
            e.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(255);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");
        });

        b.Entity<PtSession>(e =>
        {
            e.ToTable("pt_sessions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.MemberId).HasColumnName("member_id");
            e.Property(x => x.CoachId).HasColumnName("coach_id");
            e.Property(x => x.SessionDate).HasColumnName("session_date").HasColumnType("date");
            e.Property(x => x.StartTime).HasColumnName("start_time").HasColumnType("time");
            e.Property(x => x.EndTime).HasColumnName("end_time").HasColumnType("time");
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(15).HasDefaultValue("PENDING");
            e.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(255);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");
            e.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId);
            e.HasOne(x => x.Coach).WithMany().HasForeignKey(x => x.CoachId);
        });

        b.Entity<Message>(e =>
        {
            e.ToTable("messages");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.SenderUserId).HasColumnName("sender_user_id");
            e.Property(x => x.RecipientUserId).HasColumnName("recipient_user_id");
            e.Property(x => x.Body).HasColumnName("body").HasMaxLength(2000).IsRequired();
            e.Property(x => x.SentAt).HasColumnName("sent_at").HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");
            e.Property(x => x.IsRead).HasColumnName("is_read").HasDefaultValue(false);
            e.HasOne(x => x.Sender).WithMany().HasForeignKey(x => x.SenderUserId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Recipient).WithMany().HasForeignKey(x => x.RecipientUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
