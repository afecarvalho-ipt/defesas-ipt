using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Schedules.Data;
using Schedules.Extensions;
using Schedules.Models;
using Schedules.Utils;

namespace Schedules.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly SchedulesDb db;

        public ReservationsController(SchedulesDb db)
        {
            this.db = db;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SchedulesRoles.Student)]
        public IActionResult CancelReservation(CancelReservationModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                TempData["Message"] = "Dados de reserva de turno inválidos.";
                return RedirectToScheduleList();
            }

            var slot = db.ScheduleSlots
                .Include(sl => sl.Students)
                .Include(sl => sl.Schedule)
                .FirstOrDefault(sl => sl.Id == model.SlotId.Value);

            if (slot == null)
            {
                TempData["Message"] = "O turno especificado não existe.";
                return RedirectToScheduleDetails(model.ScheduleId.Value);
            }

            if (slot.Schedule.When < DateTime.Now.Date)
            {
                TempData["Message"] = "Este horário está fechado.";
                return RedirectToScheduleDetails(model.ScheduleId.Value);
            }

            var studentNumber = User.GetStudentNumber();

            if (slot.ReservedBy_Id != studentNumber)
            {
                TempData["Message"] = "Só pode cancelar turnos reservados por si.";
                return RedirectToScheduleDetails(model.ScheduleId.Value);
            }

            slot.ReservedBy_Id = null;
            slot.ReservedAt = null;
            slot.Students.Clear();

            db.ScheduleSlots.Update(slot);

            db.SaveChanges();

            return RedirectToScheduleDetails(model.ScheduleId.Value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SchedulesRoles.Student)]
        public IActionResult ReserveSlot(ReserveSlotModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                TempData["Message"] = "Dados de reserva de turno inválidos.";
                return RedirectToScheduleList();
            }

            var slot = db.ScheduleSlots
                .Include(sl => sl.Schedule)
                .Include(sl => sl.Students)
                .FirstOrDefault(sl => sl.Id == model.SlotId.Value);

            if (slot == null || !slot.IsAvailable)
            {
                TempData["Message"] = "O turno especificado não existe, ou não está disponível.";
                return RedirectToScheduleDetails(model.ScheduleId.Value);
            }

            if (slot.Schedule.When < DateTime.Now.Date)
            {
                TempData["Message"] = "Este horário está fechado.";
                return RedirectToScheduleDetails(model.ScheduleId.Value);
            }

            if (slot.ReservedBy_Id != null)
            {
                TempData["Message"] = "Este turno já está reservado. Escolha outro turno.";
                return RedirectToScheduleDetails(model.ScheduleId.Value);
            }

            var studentNumber = User.GetStudentNumber();

            var studentNumbers = new List<string> { studentNumber };
            studentNumbers.AddRange(model.OtherStudents);

            var students = db.Students
                .Where(st => studentNumbers.Contains(st.StudentNumber))
                .ToList();

            if (students.Count > slot.Schedule.MaxStudentsPerSlot)
            {
                TempData["Message"] = "Foram seleccionados demasiados alunos.";
                return RedirectToScheduleDetails(model.ScheduleId.Value);
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

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                TempData["Message"] = "Outra pessoa reservou o turno que escolheu. Escolha outro turno.";
                return RedirectToScheduleDetails(model.ScheduleId.Value);
            }

            return RedirectToScheduleDetails(model.ScheduleId.Value);
        }

        private IActionResult RedirectToScheduleDetails(long scheduleId)
        {
            return RedirectToAction("Details", "Schedules", new { id = scheduleId });
        }

        private IActionResult RedirectToScheduleList()
        {
            return RedirectToAction("Index", "Schedules");
        }

    }
}