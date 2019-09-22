using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Schedules.Data
{
    public class Schedule
    {
        public long Id { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(1024)]
        public string Description { get; set; }

        [Required]
        [StringLength(128)]
        public string Location { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatedBy { get; set; }

        public int MaxStudentsPerSlot { get; set; }

        public DateTime When { get; set; }

        public ICollection<Schedule_Student> Students { get; set; }

        public ICollection<ScheduleSlot> Slots { get; set; }
    }

    public class ScheduleSlot
    {
        public long Id { get; set; }

        [StringLength(512)]
        public string Description { get; set; }

        public bool IsAvailable { get; set; }

        public DateTime StartsAt { get; set; }

        public DateTime EndsAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? ReservedAt { get; set; }

        public string ReservedBy_Id { get; set; }

        [ForeignKey(nameof(ReservedBy_Id))]
        public Student ReservedBy { get; set; }

        public ICollection<ScheduleSlot_Student> Students { get; set; }

        [ForeignKey(nameof(Schedule_Id))]
        public Schedule Schedule { get; set; }

        public long Schedule_Id { get; set; }
    }

    public class Student
    {
        [Required, Key]
        public string StudentNumber { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        public ICollection<Schedule_Student> Schedules { get; set; }

        public ICollection<ScheduleSlot_Student> Slots { get; set; }

        public ICollection<ScheduleSlot> SlotsReservedByThisStudent { get; set; }
    }

    public class Schedule_Student
    {
        public string Student_Id { get; set; }

        public long Schedule_Id { get; set; }

        [ForeignKey(nameof(Student_Id))]
        public Student Student { get; set; }

        [ForeignKey(nameof(Schedule_Id))]
        public Schedule Schedule { get; set; }
    }

    public class ScheduleSlot_Student
    {
        public string Student_Id { get; set; }

        public long ScheduleSlot_Id { get; set; }

        [ForeignKey(nameof(Student_Id))]
        public Student Student { get; set; }

        [ForeignKey(nameof(ScheduleSlot_Id))]
        public ScheduleSlot ScheduleSlot { get; set; }
    }
}