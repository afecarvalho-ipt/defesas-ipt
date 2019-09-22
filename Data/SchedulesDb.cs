using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Schedules.Data
{
    public class SchedulesDb : DbContext
    {
        public SchedulesDb(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }

        public DbSet<Schedule> Schedules { get; set; }

        public DbSet<ScheduleSlot> ScheduleSlots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>()
                .HasIndex(p => p.StudentNumber)
                .IsUnique(true);

            modelBuilder.Entity<ScheduleSlot_Student>()
                .HasKey(p => new { p.ScheduleSlot_Id, p.Student_Id });

            modelBuilder.Entity<Schedule_Student>()
                .HasKey(p => new { p.Student_Id, p.Schedule_Id });
        }
    }
}