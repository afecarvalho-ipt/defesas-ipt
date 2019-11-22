using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Schedules.Models
{
    public class ScheduleListModel
    {
        public ICollection<ScheduleListModelItem> CurrentSchedules { get; set; }
        public ICollection<ScheduleListModelItem> PastSchedules { get; set; }
    }

    public class ScheduleListModelItem
    {
        public long Id { get; set; }

        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Display(Name = "Local")]
        public string Location { get; set; }

        [Display(Name = "Criado por")]
        public string CreatedBy { get; set; }

        [Display(Name = "Quando")]
        [DataType(DataType.Date)]
        public DateTime When { get; set; }
    }
}
