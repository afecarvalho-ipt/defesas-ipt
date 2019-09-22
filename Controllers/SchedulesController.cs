using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
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
    [Authorize]
    public class SchedulesController : Controller
    {
        private readonly SchedulesDb db;

        public SchedulesController(SchedulesDb db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            ICollection<Schedule> schedules;

            if (User.IsInRole(Roles.Faculty))
            {
                schedules = db.Schedules
                    .Where(s => s.CreatedBy == User.Identity.Name)
                    .ToList();
            }
            else if (User.IsInRole(Roles.Student))
            {
                var studentNumber = User.GetStudentNumber();

                schedules = db.Schedules
                    .Where(s => s.Students.Any(st => st.Student.StudentNumber == studentNumber))
                    .ToList();
            }
            else
            {
                return View("Unauthorized");
            }

            return View(schedules);
        }

        [HttpGet]
        [Authorize(Roles = Roles.Faculty)]
        public IActionResult Create()
        {
            return View(new ScheduleCreateModel());
        }

        [HttpPost]
        [Authorize(Roles = Roles.Faculty)]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ScheduleCreateModel model)
        {
            if (!ModelState.IsValid) { return View(model); }

            var students = new List<Schedule_Student>();
            var tempFile = System.IO.Path.GetTempFileName();
            var when = model.When.Value.Date;

            var whenAsString = when.ToString("yyyy-MM-dd");

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
                        FilterRow = (rowReader) => !string.IsNullOrWhiteSpace(rowReader.GetString(0)),
                        FilterColumn = (rowReader, columnIndex) => columnIndex <= 12
                    }
                });

                var table = dataSet.Tables[0];

                foreach (DataRow row in table.Rows)
                {
                    var studentNumber = row["Aluno"].ToString();
                    var studentName = row["Nome"].ToString();

                    // TODO: Optimize (can this be done with only one query?)
                    var student = db.Students.Find(studentNumber);

                    if (student == null)
                    {
                        student = new Student { StudentNumber = studentNumber, Name = studentName };
                        db.Students.Add(student);
                    }

                    students.Add(new Schedule_Student
                    {
                        Student = student,
                        Student_Id = studentNumber,
                        Schedule = schedule
                    });
                }
            }

            db.Schedules.Add(schedule);

            db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Student)]
        public IActionResult CancelReservation(CancelReservationModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Dados de reserva de turno inválidos.";
                return RedirectToAction(nameof(Index));
            }

            var slot = db.ScheduleSlots
                .Include(sl => sl.Students)
                .FirstOrDefault(sl => sl.Id == model.SlotId.Value);

            if (slot == null)
            {
                TempData["Message"] = "O turno especificado não existe.";
                return Details(model.ScheduleId.Value);
            }

            var studentNumber = User.GetStudentNumber();

            if (slot.ReservedBy_Id != studentNumber)
            {
                TempData["Message"] = "Só pode cancelar turnos reservados por si.";
                return Details(model.ScheduleId.Value);
            }

            slot.ReservedBy_Id = null;
            slot.ReservedAt = null;
            slot.Students.Clear();

            db.ScheduleSlots.Update(slot);

            db.SaveChanges();

            return RedirectToAction("Details", new { id = model.ScheduleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Student)]
        public IActionResult ReserveSlot(ReserveSlotModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Dados de reserva de turno inválidos.";
                return RedirectToAction(nameof(Index));
            }

            var slot = db.ScheduleSlots
                .Include(sl => sl.Schedule)
                .Include(sl => sl.Students)
                .FirstOrDefault(sl => sl.Id == model.SlotId.Value);

            if (slot == null)
            {
                TempData["Message"] = "O turno especificado não existe.";
                return Details(model.ScheduleId.Value); // No such slot
            }

            var studentNumber = User.GetStudentNumber();

            if (slot.ReservedBy_Id != null)
            {
                TempData["Message"] = "Este turno já está reservado. Escolha outro turno.";
                return Details(model.ScheduleId.Value); // Already reserved.
            }

            var studentNumbers = new List<string> { studentNumber };
            studentNumbers.AddRange(model.OtherStudents);

            var students = db.Students.Where(st => studentNumbers.Contains(st.StudentNumber)).ToList();

            if (students.Count > slot.Schedule.MaxStudentsPerSlot)
            {
                TempData["Message"] = "Foram seleccionados demasiados alunos.";
                return Details(model.ScheduleId.Value); // Too many students.
            }

            slot.ReservedAt = DateTime.Now;
            slot.ReservedBy_Id = studentNumber;

            slot.Students = students
                .Select(st => new ScheduleSlot_Student
                {
                    ScheduleSlot = slot,
                    Student = st
                })
                .ToList();

            db.ScheduleSlots.Update(slot);

            db.SaveChanges();

            return RedirectToAction("Details", new { id = model.ScheduleId });
        }

        public IActionResult Details(long id)
        {
            var studentNumber = User.GetStudentNumber();

            var schedule = db.Schedules
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
                                .ToList(),

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

            if (schedule == null) { return View("ScheduleNotFound"); }

            return View(schedule);
        }
    }
}