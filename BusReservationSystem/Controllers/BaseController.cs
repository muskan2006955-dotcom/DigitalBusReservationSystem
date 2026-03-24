using Microsoft.AspNetCore.Mvc;
using BusReservationSystem.Models;

namespace BusReservationSystem.Controllers
{
    public class BaseController : Controller
    {
        protected readonly BusReserveDbContext _context;

        public BaseController(BusReserveDbContext context)
        {
            _context = context;
        }

        // Ye function ab har us controller mein available hoga jo BaseController se inherit karega
        protected async Task LogActivity(string action, string module, string details)
        {
            var log = new SystemLog
            {
                AdminEmail = HttpContext.Session.GetString("AdminEmail") ?? "System",
                Action = action,
                Module = module,
                Details = details,
                Timestamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.SystemLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}