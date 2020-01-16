using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Schedules.Models
{
    public class ScheduleDetailsModel
    {
        public long Id { get; set; }

        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Local")]
        public string Location { get; set; }

        [Display(Name = "Criado por")]
        public string CreatedBy { get; set; }

        [Display(Name = "Nº máximo de estudantes por turno")]
        public int MaxStudentsPerSlot { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Quando")]
        public DateTime When { get; set; }

        public ICollection<StudentDisplayModel> Students { get; set; }

        public ICollection<ScheduleSlotDetailsModel> Slots { get; set; }
    }

    public class ScheduleSlotDetailsModel
    {
        public long Id { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Turno")]
        public bool IsAvailable { get; set; }

        public bool ReservedByCurrentUser { get; set; }

        public string ReservedBy { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Início")]
        public DateTime StartsAt { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Fim")]
        public DateTime EndsAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public ICollection<StudentDisplayModel> Students { get; set; }
    }

    public class StudentDisplayModel
    {
        [Display(Name = "Nº de aluno")]
        public string StudentNumber { get; set; }

        [Display(Name = "Nome")]
        public string Name { get; set; }
    }
}