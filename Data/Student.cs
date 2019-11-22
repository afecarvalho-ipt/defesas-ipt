using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Schedules.Data
{
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
}