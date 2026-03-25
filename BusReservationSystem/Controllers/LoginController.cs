using BusReservationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusReservationSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly BusReserveDbContext context;

        public LoginController(BusReserveDbContext context)
        {
            this.context = context;
        }

        // 1. LogActivity Method (Sirf ek baar yahan rakhen)
        private async Task LogActivity(string action, string module, string details)
        {
            var log = new SystemLog
            {
                AdminEmail = HttpContext.Session.GetString("Name") ?? "Guest",
                Action = action,
                Module = module,
                Details = details,
                Timestamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            context.SystemLogs.Add(log);
            await context.SaveChangesAsync();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = context.Employees
                .FirstOrDefault(x => x.Username == username && x.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("UserID", user.EmployeeId.ToString());
                HttpContext.Session.SetString("Name", user.FirstName);

                // Success Log
                await LogActivity("Login", "Auth", $"User {user.FirstName} ({user.Role}) logged in successfully.");

                if (user.Role == "Admin")
                    return RedirectToAction("AdminDashboard", "Home");
                else
                    return RedirectToAction("EmployeeDashboard", "Home");
            }

            // Failure Log
            await LogActivity("Login Failed", "Auth", $"Failed attempt with username: {username}");

            ViewBag.Error = "Invalid Username or Password";
            return View("Login");
        }

        // 2. Logout Method (Sirf ek baar yahan rakhen)
        public async Task<IActionResult> Logout()
        {
            // Log pehle karein taake session se naam mil sake
            await LogActivity("Logout", "Auth", "User logged out.");

            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Login");
        }
    }
}