using Habit_Tracker_Backend.Models.Classes;
using Microsoft.EntityFrameworkCore;

namespace Habit_Tracker_Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // ---------------- AUTH ----------------
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserOtp> UserOtps { get; set; } = null!;

        public DbSet<Feedback> Feedback { get; set; } = null!;

        // ---------------- HABITS ----------------
        public DbSet<HabitCategory> HabitCategories { get; set; } = null!;
        public DbSet<Habit> Habits { get; set; } = null!;
        public DbSet<HabitSchedule> HabitSchedules { get; set; } = null!;
        public DbSet<HabitLog> HabitLogs { get; set; } = null!;
        public DbSet<HabitStreak> HabitStreaks { get; set; } = null!;
        public DbSet<HabitReminder> HabitReminders { get; set; } = null!;

       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
            modelBuilder.Entity<User>()
                .ToTable("users")
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<UserOtp>()
                .ToTable("user_otp")
                .Property(o => o.OtpType)
                .HasConversion<string>();

            modelBuilder.Entity<UserOtp>()
                .ToTable("user_otp")
                .Property(o => o.Channel)
                .HasConversion<string>();

            modelBuilder.Entity<HabitLog>()
                .ToTable("habit_log")
                .Property(h => h.Status)
                .HasConversion<string>();

            modelBuilder.Entity<HabitSchedule>()
                .ToTable("habit_schedule")
                .Property(s => s.HabitDayOfWeek)
                .HasConversion<string>();

            modelBuilder.Entity<Habit>()
                .ToTable("habits")
                .Property(h => h.Priority)
                .HasConversion<string>();

            modelBuilder.Entity<HabitReminder>()
        .ToTable("habit_reminder");

    modelBuilder.Entity<HabitCategory>()
        .ToTable("habit_categories");

    modelBuilder.Entity<Feedback>()
        .ToTable("feedback");
            modelBuilder.Entity<HabitLog>()
                .Property(x => x.LogDate)
                .HasConversion(
                    d => d.ToDateTime(TimeOnly.MinValue),
                    d => DateOnly.FromDateTime(d)
                );

            modelBuilder.Entity<HabitStreak>()
                .ToTable("habit_streaks")
                .Property(x => x.LastCompletedDate)
                .HasConversion(
                    d => d.HasValue ? d.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    d => d.HasValue ? DateOnly.FromDateTime(d.Value) : null
                );

            

            // User → Habits (1:N)
            modelBuilder.Entity<Habit>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Habit → Category (N:1)
            modelBuilder.Entity<Habit>()
                .HasOne(h => h.HabitCategory)
                .WithMany(c => c.Habits)
                .HasForeignKey(h => h.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Habit → Schedules (1:N)
            modelBuilder.Entity<HabitSchedule>()
                .HasOne<Habit>()
                .WithMany(h => h.HabitSchedules)
                .HasForeignKey(s => s.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Habit → Logs (1:N)
            modelBuilder.Entity<HabitLog>()
                .HasOne<Habit>()
                .WithMany(h => h.HabitLogs)
                .HasForeignKey(l => l.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Habit → Streak (1:1)
            modelBuilder.Entity<Habit>()
                .HasOne(h => h.HabitStreak)
                .WithOne(s => s.Habit)
                .HasForeignKey<HabitStreak>(s => s.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Habit → Reminder (1:1)
            modelBuilder.Entity<HabitReminder>()
                .HasOne<Habit>()
                .WithOne(h => h.HabitReminder)
                .HasForeignKey<HabitReminder>(r => r.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<Habit>()
                .HasIndex(h => new { h.UserId, h.HabitName })
                .IsUnique();

            modelBuilder.Entity<HabitSchedule>()
                .HasIndex(s => new { s.HabitId, s.HabitDayOfWeek })
                .IsUnique();

            modelBuilder.Entity<HabitLog>()
                .HasIndex(l => new { l.HabitId, l.LogDate })
                .IsUnique();

            modelBuilder.Entity<HabitStreak>()
                .HasIndex(s => s.HabitId)
                .IsUnique();

            modelBuilder.Entity<HabitReminder>()
                .HasIndex(r => r.HabitId)
                .IsUnique();
        }


    }
}
