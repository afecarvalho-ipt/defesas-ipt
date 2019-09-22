using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Schedules.Models
{
    public class ScheduleDetailsModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public string CreatedBy { get; set; }

        public int MaxStudentsPerSlot { get; set; }

        [DataType(DataType.Date)]
        public DateTime When { get; set; }

        public ICollection<StudentDisplayModel> Students { get; set; }

        public ICollection<ScheduleSlotDetailsModel> Slots { get; set; }
    }

    public class ScheduleSlotDetailsModel
    {
        public long Id { get; set; }

        public string Description { get; set; }

        public bool IsAvailable { get; set; }

        public bool ReservedByCurrentUser { get; set; }

        [DataType(DataType.Time)]
        public DateTime StartsAt { get; set; }

        [DataType(DataType.Time)]
        public DateTime EndsAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public ICollection<StudentDisplayModel> Students { get; set; }
    }

    public class StudentDisplayModel
    {
        public string StudentNumber { get; set; }

        public string Name { get; set; }
    }
}