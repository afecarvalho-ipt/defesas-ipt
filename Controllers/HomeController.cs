using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Schedules.Models;

namespace Schedules.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties { RedirectUri = Url.RouteUrl("Index") }, "Cookies", "AzureOidc");
        }

        public IActionResult Login()
        {
            // Prevent a redirect loop.
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }

            return Challenge(new AuthenticationProperties { RedirectUri = Url.RouteUrl("Index") });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error([FromServices] IHostingEnvironment env)
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Exception = context.Error,
                IsDevelopment = env.IsDevelopment()
            });
        }
    }
}
