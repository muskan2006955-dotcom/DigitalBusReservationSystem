using System.Diagnostics;
using BusReservationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusReservationSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly BusReserveDbContext _context;

        public HomeController(BusReserveDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        public async Task<IActionResult> EmployeeDashboard()
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            int loggedInId = int.Parse(userIdStr);

            // Database se employee ki mukammal info nikalna (Naam aur Picture ke liye)
            var employee = await _context.Employees.FindAsync(loggedInId);

            var viewModel = new EmployeeDashboardViewModel
            {
                // Yahan asli naam set ho raha hy
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Employee",

                // Picture ka naam ViewModel mein pass karna (ViewModel mein ye property add karni hogi)
                ProfilePicture = employee?.ProfilePicture ?? "default-user.png",

                TotalMyCustomers = await _context.Customers.CountAsync(c => c.RegisteredBy == loggedInId),
                TotalMyBookings = await _context.Bookings.CountAsync(b => b.BookedBy == loggedInId),
                TotalMyEarnings = await _context.Bookings
                    .Where(b => b.BookedBy == loggedInId)
                    .SumAsync(b => (decimal?)b.FinalAmount ?? 0),
                TotalBusesAvailable = await _context.Buses.CountAsync(b => b.Status == "Active"),
                RecentBookings = await _context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.Bus)
                    .Where(b => b.BookedBy == loggedInId)
                    .OrderByDescending(b => b.BookingId)
                    .Take(5).ToListAsync(),
                RecentCustomers = await _context.Customers
                    .Where(c => c.RegisteredBy == loggedInId)
                    .OrderByDescending(c => c.CustomerId)
                    .Take(5).ToListAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> AdminDashboard()
        {
            // 1. Session Check (Matching LoginController key "UserRole")
            var userIdStr = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userIdStr) || userRole != "Admin")
            {
                return RedirectToAction("Login", "Login");
            }

            int loggedInId = int.Parse(userIdStr);
            var admin = await _context.Employees.FindAsync(loggedInId);

            // 2. Fetching Data for ViewModel
            var viewModel = new AdminDashboardViewModel
            {
                AdminName = admin != null ? $"{admin.FirstName} {admin.LastName}" : "System Admin",
                ProfilePicture = admin?.ProfilePicture ?? "default-user.png",

                TotalRevenue = await _context.Bookings
                    .Where(b => b.BookingStatus != "Cancelled")
                    .SumAsync(b => (decimal?)b.FinalAmount ?? 0),

                TotalBuses = await _context.Buses.CountAsync(b => b.Status == "Active"),

                ActiveAgents = await _context.Employees.CountAsync(e => e.Role == "Employee"),

                TotalCancellations = await _context.Bookings.CountAsync(b => b.BookingStatus == "Cancelled"),

                // Important: Include zarur karein taake related data null na ho
                RecentSystemBookings = await _context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.Bus)
                    .Include(b => b.BookedByNavigation)
                    .OrderByDescending(b => b.BookingId)
                    .Take(5).ToListAsync()
            };

            return View(viewModel); // ViewModel bhej rahe hain
        }
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}