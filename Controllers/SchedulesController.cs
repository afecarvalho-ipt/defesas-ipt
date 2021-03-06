using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Schedules.Data;
using Schedules.Extensions;
using Schedules.Models;
using Schedules.Utils;

namespace Schedules.Controllers
{
    [Authorize(Roles = SchedulesRoles.Faculty + "," + SchedulesRoles.Student)]
    public class SchedulesController : Controller
    {
        private readonly SchedulesDb db;

        public SchedulesController(SchedulesDb db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            var schedules = CurrentUserSchedules();

            var today = DateTime.Now.Date;

            // [true] -> Current and future schedules
            // [false] -> Past schedules
            var groupedSchedulesByAvailability = schedules
                .AsEnumerable() // The query can't be done via SQL
                .GroupBy(
                    s => s.When >= today,
                    s => new ScheduleListModelItem
                    {
                        Id = s.Id,
                        Name = s.Name,
                        When = s.When,
                        CreatedBy = s.CreatedBy,
                        Location = s.Location
                    })
                .ToDictionary(
                    s => s.Key,
                    s => s.OrderByDescending(x => x.When).ToList()
                );

            var model = new ScheduleListModel
            {
                CurrentSchedules = groupedSchedulesByAvailability.GetValueOrDefault(true, new List<ScheduleListModelItem>()),
                PastSchedules = groupedSchedulesByAvailability.GetValueOrDefault(false, new List<ScheduleListModelItem>()),
            };

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = SchedulesRoles.Faculty)]
        public IActionResult Create()
        {
            return View(new ScheduleCreateModel());
        }

        [HttpPost]
        [Authorize(Roles = SchedulesRoles.Faculty)]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ScheduleCreateModel model)
        {
            if (model == null || !ModelState.IsValid) { return View(model); }

            var students = new List<Schedule_Student>();
            var tempFile = System.IO.Path.GetTempFileName();
            var when = model.When.Value.Date;

            var whenAsString = when.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            var schedule = new Schedule
            {
                CreatedBy = User.Identity.Name,
                Description = model.Description,
                Location = model.Location,
                MaxStudentsPerSlot = model.MaxStudentsPerSlot.Value,
                Name = model.Name,
                When = when,
                Students = students,
                Slots = model.Slots
                    .OrderBy(s => s.StartsAt)
                    .Select(s => new ScheduleSlot
                    {
                        IsAvailable = s.Available,
                        StartsAt = DateTime.ParseExact($"{whenAsString} {s.StartsAt}", "yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal),
                        EndsAt = DateTime.ParseExact($"{whenAsString} {s.EndsAt}", "yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal),
                        Description = s.Description,
                    })
                    .ToList()
            };

            // Read students.
            using (var reader = ExcelReaderFactory.CreateReader(model.StudentsUpload.OpenReadStream()))
            {
                // https://github.com/ExcelDataReader/ExcelDataReader#asdataset-configuration-options
                var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    FilterSheet = (tableReader, sheetIndex) => sheetIndex == 0,
                    ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true,
                        FilterRow = (rowReader) => !string.IsNullOrWhiteSpace(rowReader.GetValue(0)?.ToString()),
                        FilterColumn = (rowReader, columnIndex) => columnIndex <= 12,
                        ReadHeaderRow = (rowReader) =>
                        {
                            // Only try to read the first 100 columns; bail otherwise as the file may be bad.
                            var rowIndex = 0;

                            while (rowIndex < 100 && string.IsNullOrWhiteSpace(rowReader.GetValue(0)?.ToString()))
                            {
                                rowReader.Read();
                                rowIndex += 1;
                            }
                        }
                    }
                });

                var table = dataSet.Tables[0];

                if (!table.Columns.Contains("Aluno") || !table.Columns.Contains("Nome"))
                {
                    ModelState.AddModelError(nameof(model.StudentsUpload), "O ficheiro Excel tem que conter uma coluna 'Aluno' e outra 'Nome'.");
                    return View(model);
                }

                var studentsInExcel = table.Rows
                    .OfType<DataRow>()
                    .Select(row =>
                    {
                        var studentNumber = row["Aluno"].ToString().Trim();
                        var studentName = row["Nome"].ToString().Trim();

                        return new Student { StudentNumber = studentNumber, Name = studentName };
                    })
                    // Remove empty rows
                    .Where(s => !string.IsNullOrWhiteSpace(s.Name) && !string.IsNullOrWhiteSpace(s.StudentNumber))
                    // Remove duplicates
                    .Distinct(new LambdaEqualityComparer<Student, string>(s => s.StudentNumber))
                    .ToList();

                // Get the student numbers that are both in the db and the Excel spreadsheet.
                // For those that aren't in the db (i.e., not in this variable), add them to the db.
                var studentsInExcelAndDb = db.Students
                    .Where(s => studentsInExcel.Select(e => e.StudentNumber).ToList().Contains(s.StudentNumber))
                    .Select(s => s.StudentNumber)
                    .ToHashSet();

                foreach (var inExcel in studentsInExcel)
                {
                    var studentNumber = inExcel.StudentNumber;

                    if (!studentsInExcelAndDb.Contains(studentNumber))
                    {
                        db.Students.Add(new Student { StudentNumber = studentNumber, Name = inExcel.Name });
                    }

                    students.Add(new Schedule_Student
                    {
                        Student_Id = studentNumber,
                        Schedule = schedule
                    });
                }
            }

            db.Schedules.Add(schedule);

            db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(long id)
        {
            var schedule = GetScheduleDetailsModel(id);

            if (schedule == null) { return View("ScheduleNotFound"); }

            return View(schedule);
        }

        [HttpGet]
        [Authorize(Roles = SchedulesRoles.Faculty)]
        public IActionResult Print(long id)
        {
            var schedule = GetScheduleDetailsModel(id);

            if (schedule == null) { return View("ScheduleNotFound"); }

            return View(schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SchedulesRoles.Faculty)]
        public IActionResult Delete([FromForm] long id)
        {
            var schedule = CurrentUserSchedules()
                .FirstOrDefault(x => x.Id == id);

            if (schedule == null) { return View("ScheduleNotFound"); }

            if (schedule.When.Date < DateTime.Now.Date)
            {
                return View("Unauthorized");
            }

            db.Schedules.Remove(schedule);

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        private IQueryable<Schedule> CurrentUserSchedules()
        {
            IQueryable<Schedule> schedules = Enumerable.Empty<Schedule>().AsQueryable();

            if (User.IsInRole(SchedulesRoles.Faculty))
            {
                // Faculty can only read the schedules they created.
                schedules = db.Schedules
                    .Where(s => s.CreatedBy == User.Identity.Name);
            }
            else if (User.IsInRole(SchedulesRoles.Student))
            {
                // Students can only see schedules they've been added to.
                var studentNumber = User.GetStudentNumber();

                schedules = db.Schedules
                    .Where(s => s.Students.Any(st => st.Student.StudentNumber == studentNumber));
            }

            return schedules;
        }

        private ScheduleDetailsModel GetScheduleDetailsModel(long id)
        {
            var studentNumber = User.GetStudentNumber();

            var schedule = CurrentUserSchedules()
                .Where(s => s.Id == id)
                .Select(s => new ScheduleDetailsModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    CreatedBy = s.CreatedBy,
                    Description = s.Description,
                    Location = s.Location,
                    MaxStudentsPerSlot = s.MaxStudentsPerSlot,
                    Slots = s.Slots
                        .OrderBy(sl => sl.StartsAt)
                        .Select(sl => new ScheduleSlotDetailsModel
                        {
                            Id = sl.Id,
                            CompletedAt = sl.CompletedAt,
                            Description = sl.Description,
                            ReservedBy = sl.ReservedBy_Id,
                            ReservedByCurrentUser = sl.ReservedAt != null && sl.ReservedBy_Id == studentNumber,
                            EndsAt = sl.EndsAt,
                            IsAvailable = sl.IsAvailable,
                            StartsAt = sl.StartsAt,
                            Students = sl.Students
                                .OrderBy(st => st.Student_Id)
                                .Select(st => new StudentDisplayModel
                                {
                                    StudentNumber = st.Student.StudentNumber,
                                    Name = st.Student.Name
                                })
                                .ToList()
                        })
                        .ToList(),
                    When = s.When,
                    Students = s.Students
                        .OrderBy(st => st.Student_Id)
                        .Select(st => new StudentDisplayModel
                        {
                            StudentNumber = st.Student.StudentNumber,
                            Name = st.Student.Name
                        })
                        .ToList()
                })
                .SingleOrDefault();

            return schedule;
        }
    }
}