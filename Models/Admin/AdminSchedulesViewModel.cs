using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Schedules.Models.Admin
{
    public class AdminSchedulesViewModel
    {
        public ICollection<AdminSchedulesViewModelItem> Schedules { get; internal set; }
    }

    public class AdminSchedulesViewModelItem
    {
        public long Id { get; internal set; }
        public string Name { get; internal set; }
        public string CreatedBy { get; internal set; }

        [DataType(DataType.Date)]
        public DateTime When { get; internal set; }
        public int StudentCount { get; internal set; }
    }
}