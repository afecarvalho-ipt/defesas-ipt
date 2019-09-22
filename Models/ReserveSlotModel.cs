using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Schedules.Models
{
    public class ReserveSlotModel
    {
        [Required]
        public long? ScheduleId { get; set; }

        [Required]
        public long? SlotId { get; set; }

        public ICollection<string> OtherStudents { get; set; } = new HashSet<string>();
    }

    public class CancelReservationModel
    {
        [Required]
        public long? ScheduleId { get; set; }

        [Required]
        public long? SlotId { get; set; }
    }
}