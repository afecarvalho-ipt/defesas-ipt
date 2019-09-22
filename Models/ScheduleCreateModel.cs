using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Schedules.Models
{
    public class ScheduleCreateModel : IValidatableObject
    {
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(1024)]
        public string Description { get; set; }

        [Required]
        [StringLength(128)]
        public string Location { get; set; }

        [Required]
        public int? MaxStudentsPerSlot { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? When { get; set; }

        [Required]
        public IFormFile StudentsUpload { get; set; }

        [Required]
        public IList<ScheduleSlotCreateModel> Slots { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var fileName = StudentsUpload.FileName.ToLowerInvariant();
            var extension = Path.GetExtension(fileName);
            var contentType = StudentsUpload.ContentType;

            var allowedExtensions = new[] { ".xls", ".xlsx" };
            var allowedContentTypes = new[] { "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" };

            if (!allowedExtensions.Contains(extension) && !allowedContentTypes.Contains(contentType))
            {
                yield return new ValidationResult("Só são permitidos ficheiros Excel (.xls ou .xlsx)");
            }
        }
    }

    public class ScheduleSlotCreateModel
    {
        [Required]
        [RegularExpression(@"^\d{2}:\d{2}$")]
        public string StartsAt { get; set; }

        [Required]
        [RegularExpression(@"^\d{2}:\d{2}$")]
        public string EndsAt { get; set; }

        [Required]
        public bool Available { get; set; }

        [StringLength(512)]
        public string Description { get; set; }
    }
}