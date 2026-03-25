using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusReservationSystem.Models;

namespace BusReservationSystem.Controllers
{
    public class SystemLogController : Controller
    {
        private readonly BusReserveDbContext _context;

        public SystemLogController(BusReserveDbContext context)
        {
            _context = context;
        }

        // GET: SystemLog
        public async Task<IActionResult> Index(string searchTerm, string moduleFilter)
        {
            var logsQuery = _context.SystemLogs.AsQueryable();

            // Search Logic (Email ya Details mein search karein)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                logsQuery = logsQuery.Where(l => l.AdminEmail.Contains(searchTerm) || l.Details.Contains(searchTerm));
            }

            // Module Filter Logic
            if (!string.IsNullOrEmpty(moduleFilter))
            {
                logsQuery = logsQuery.Where(l => l.Module == moduleFilter);
            }

            var logs = await logsQuery
                .OrderByDescending(l => l.Timestamp)
                .Take(500) // Performance ke liye sirf latest 500
                .ToListAsync();

            // Dropdown ke liye unique modules list
            ViewBag.Modules = await _context.SystemLogs
                .Select(l => l.Module)
                .Distinct()
                .ToListAsync();

            return View(logs);
        }

        // POST: Purge Logs (Older than 30 days)
        [HttpPost]
        public async Task<IActionResult> PurgeLogs()
        {
            var cutoff = DateTime.Now.AddDays(-30);
            var oldLogs = _context.SystemLogs.Where(l => l.Timestamp < cutoff);

            _context.SystemLogs.RemoveRange(oldLogs);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}