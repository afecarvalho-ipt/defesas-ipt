using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Schedules.Data;
using Schedules.Models.Admin;
using Schedules.Utils;

namespace Schedules.Controllers
{
    [Authorize(Roles = SchedulesRoles.Admin)]
    public class AdminController : Controller
    {
        private readonly SchedulesDb db;

        public AdminController(SchedulesDb db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Schedules()
        {
            var schedules = await db.Schedules
                .OrderByDescending(s => s.Id)
                .Select(s => new AdminSchedulesViewModelItem
                {
                    Id = s.Id,
                    Name = s.Name,
                    CreatedBy = s.CreatedBy,
                    When = s.When,
                    StudentCount = s.Students.Count,
                })
                .ToListAsync();

            return View(new AdminSchedulesViewModel
            {
                Schedules = schedules
            });
        }
    }
}
