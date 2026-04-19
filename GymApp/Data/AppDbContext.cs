using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        {
        }

        public DbSet<Member> Members { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<GymProgram> GymPrograms { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Μετατροπή enum σε string στη βάση (πιο ευανάγνωστο)
            modelBuilder.Entity<Subscription>()
                .Property(s => s.SessionType)
                .HasConversion<string>();

            modelBuilder.Entity<TimeSlot>()
                .Property(t => t.SessionType)
                .HasConversion<string>();

            modelBuilder.Entity<Booking>()
                .Property(b => b.Status)
                .HasConversion<string>();

            modelBuilder.Entity<TimeSlot>()
                .Property(t => t.StartTime)
                .HasConversion(
                    v => v.ToString(),
                    v => TimeOnly.Parse(v));

            modelBuilder.Entity<Booking>()
            .HasOne(b => b.TimeSlot)
            .WithMany(t => t.Bookings)
            .HasForeignKey(b => b.TimeSlotId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Subscription)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.SubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentMethod)
                .HasConversion<string>();

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Subscription)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
