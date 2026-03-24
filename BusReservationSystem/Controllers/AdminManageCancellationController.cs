using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusReservationSystem.Models;

namespace BusReservationSystem.Controllers
{
    public class AdminManageCancellationController : Controller
    {
        private readonly BusReserveDbContext _context;

        public AdminManageCancellationController(BusReserveDbContext context)
        {
            _context = context;
        }

        // Action for Cancelled Bookings List
        public async Task<IActionResult> Index(string searchTerm, DateTime? cancelDate)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Bus)
                .Include(b => b.BookedByNavigation)
                .Where(b => b.BookingStatus == "Cancelled")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(b =>
                    b.Customer.CustomerName.ToLower().Contains(searchTerm) ||
                    b.Customer.PhoneNumber.Contains(searchTerm) ||
                    (b.Bus != null && b.Bus.BusNumber.Contains(searchTerm)) || // Property name fix
                    (b.BookedByNavigation != null && b.BookedByNavigation.FirstName.ToLower().Contains(searchTerm))
                );
            }

            if (cancelDate.HasValue)
            {
                query = query.Where(b => b.CancellationDate.HasValue &&
                                         b.CancellationDate.Value.Date == cancelDate.Value.Date);
            }

            var results = await query.OrderByDescending(b => b.CancellationDate).ToListAsync();

            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentDate"] = cancelDate?.ToString("yyyy-MM-dd");

            return View(results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Bus)
                .Include(b => b.BookedByNavigation)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();
            return View(booking);
        }
    }
}