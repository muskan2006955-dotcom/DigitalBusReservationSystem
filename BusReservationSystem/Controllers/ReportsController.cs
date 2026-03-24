using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusReservationSystem.Models;
using Microsoft.AspNetCore.Http;

namespace BusReservationSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly BusReserveDbContext _context;

        public ReportsController(BusReserveDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
                return RedirectToAction("Login", "Login");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GenerateReport(DateTime? fromDate, DateTime? toDate)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            int loggedInId = int.Parse(userIdStr);

            // Agar user ne dates select nahi ki toh wide range rakhen testing k liye
            DateTime start = fromDate ?? new DateTime(2000, 1, 1);
            DateTime end = toDate ?? new DateTime(2099, 12, 31);

            // Time ka masla khatam karne k liye end date ko din k aakhir tak le jayen
            DateTime finalEndDate = end.Date.AddDays(1).AddTicks(-1);

            // Sab se asaan aur mazboot query
            var reportData = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Bus)
                .Where(b => b.BookedBy == loggedInId) // Pehle Employee filter
                .ToListAsync(); // Data memory mein le ayen taake C# ki date logic chale

            // Memory mein filter lagayen taake exact date match ho (Manual dates k liye best hai)
            var filteredData = reportData.Where(b => b.BookingDate.HasValue &&
                                              b.BookingDate.Value.Date >= start.Date &&
                                              b.BookingDate.Value.Date <= end.Date)
                                        .OrderByDescending(b => b.BookingDate)
                                        .ToList();

            ViewBag.From = start.ToString("dd MMM yyyy");
            ViewBag.To = end.ToString("dd MMM yyyy");
            ViewBag.EmpID = loggedInId;
            ViewBag.TotalAmount = filteredData.Sum(x => x.FinalAmount ?? 0);

            return View("ReportPrint", filteredData);
        }
    }
}