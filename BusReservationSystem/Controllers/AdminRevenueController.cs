using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusReservationSystem.Models;
using System.Text;

namespace BusReservationSystem.Controllers
{
    public class AdminRevenueController : Controller
    {
        private readonly BusReserveDbContext _context;

        public AdminRevenueController(BusReserveDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            // Login Check (Optional but recommended)
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            // 1. Date Range Setup
            // Agar dates nahi hain toh pichle 30 din ka data
            DateTime start = fromDate ?? DateTime.Now.Date.AddDays(-30);
            DateTime end = toDate ?? DateTime.Now.Date;

            // 2. Query with Safety Checks
            // .AsQueryable use karne se filter SQL level par hi lagta hai
            var query = _context.Bookings
                .Include(b => b.Bus)
                .Include(b => b.BookedByNavigation)
                .Where(b => b.BookingDate != null) // Null check lazmi hai
                .AsQueryable();

            // Date comparison with .Date for both sides
            var bookings = await query
                .Where(b => b.BookingDate.Value.Date >= start.Date &&
                            b.BookingDate.Value.Date <= end.Date)
                .OrderBy(b => b.BookingDate)
                .ToListAsync();

            // 3. Stats Calculation (Safe Sum)
            ViewBag.TotalSales = bookings.Sum(b => b.FinalAmount ?? 0);
            ViewBag.TotalRefunds = bookings.Sum(b => b.RefundAmount ?? 0);
            ViewBag.NetRevenue = (decimal)ViewBag.TotalSales - (decimal)ViewBag.TotalRefunds;
            ViewBag.TotalTickets = bookings.Count;

            // 4. Chart Logic (Grouped by Date safely)
            var chartGroup = bookings
                .GroupBy(b => b.BookingDate.Value.Date)
                .Select(g => new {
                    Date = g.Key.ToString("dd MMM"),
                    Amount = g.Sum(b => (b.FinalAmount ?? 0) - (b.RefundAmount ?? 0))
                }).ToList();

            ViewBag.ChartLabels = chartGroup.Select(x => x.Date).ToArray();
            ViewBag.ChartValues = chartGroup.Select(x => x.Amount).ToArray();

            // 5. ViewData for Form Retention
            ViewData["FromDate"] = start.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = end.ToString("yyyy-MM-dd");

            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> ExportReport(DateTime? fromDate, DateTime? toDate)
        {
            DateTime start = fromDate ?? DateTime.Now.AddDays(-30);
            DateTime end = toDate ?? DateTime.Now;

            var data = await _context.Bookings
                .Include(b => b.Bus)
                .Include(b => b.Customer)
                .Where(b => b.BookingDate != null &&
                            b.BookingDate.Value.Date >= start.Date &&
                            b.BookingDate.Value.Date <= end.Date)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("BookingID,Date,Customer,Bus,Status,FinalAmount,RefundAmount,NetEarning");

            foreach (var item in data)
            {
                decimal net = (item.FinalAmount ?? 0) - (item.RefundAmount ?? 0);
                // CSV formatting with string interpolation
                csv.AppendLine($"{item.BookingId},{item.BookingDate?.ToString("dd-MM-yyyy")},{item.Customer?.CustomerName?.Replace(",", " ")},{item.Bus?.BusNumber},{item.BookingStatus},{item.FinalAmount},{item.RefundAmount},{net}");
            }

            byte[] buffer = Encoding.UTF8.GetBytes(csv.ToString());
            return File(buffer, "text/csv", $"RevenueReport_{start:ddMMyy}_to_{end:ddMMyy}.csv");
        }
    }
}