using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusReservationSystem.Models;

namespace BusReservationSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly BusReserveDbContext _context;

        public CustomerController(BusReserveDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchTerm, DateTime? travelDate)
        {
            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Bus)
                .AsQueryable();

            // Search by text (Name/CNIC/Phone)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(b => b.Customer.CustomerName.ToLower().Contains(searchTerm)
                                      || b.Customer.IdproofNumber.Contains(searchTerm)
                                      || b.Customer.PhoneNumber.Contains(searchTerm));
            }

            // Filter by Date (sirf tab jab date provide ki jaye)
            if (travelDate.HasValue)
            {
                var dateOnly = DateOnly.FromDateTime(travelDate.Value);
                query = query.Where(b => b.TravelDate == dateOnly);
            }

            var results = await query.OrderByDescending(b => b.BookingId).ToListAsync();
            return View(results);
        }
        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
                return RedirectToAction("Login", "Login");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            // --- UNIQUE CNIC CHECK ---
            // Pehle check karein ke ye ID Proof DB mein mojud hai ya nahi
            var isDuplicate = await _context.Customers.AnyAsync(c => c.IdproofNumber == customer.IdproofNumber);
            if (isDuplicate)
            {
                ModelState.AddModelError("IdproofNumber", "This ID Proof (CNIC) is already registered.");
            }

            if (ModelState.IsValid)
            {
                customer.CreatedDate = DateTime.Now;
                customer.RegisteredBy = int.Parse(userIdStr);

                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (id == null || string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            var customer = await _context.Customers.FindAsync(id);

            if (customer == null || customer.RegisteredBy != int.Parse(userIdStr))
                return NotFound();

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (id != customer.CustomerId || string.IsNullOrEmpty(userIdStr)) return NotFound();

            // --- UNIQUE CNIC CHECK ON EDIT ---
            // Check karein ke kisi AUR bande ka ye CNIC toh nahi (ID skip karke)
            var isDuplicate = await _context.Customers.AnyAsync(c => c.IdproofNumber == customer.IdproofNumber && c.CustomerId != id);
            if (isDuplicate)
            {
                ModelState.AddModelError("IdproofNumber", "This ID Proof is already assigned to another customer.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    customer.RegisteredBy = int.Parse(userIdStr);
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (id == null || string.IsNullOrEmpty(userIdStr)) return NotFound();

            var customer = await _context.Customers
                .Include(c => c.Bookings)
                .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (customer == null || customer.RegisteredBy != int.Parse(userIdStr)) return NotFound();

            return View(customer);
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr))
                return Json(new { success = false, message = "Session expired. Please login again." });

            int loggedInId = int.Parse(userIdStr);
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
                return Json(new { success = false, message = "Customer not found." });

            // Security Check
            if (customer.RegisteredBy != loggedInId)
                return Json(new { success = false, message = "Unauthorized access." });

            // Booking Check
            var hasBookings = await _context.Bookings.AnyAsync(b => b.CustomerId == id);
            if (hasBookings)
                return Json(new { success = false, message = "Cannot delete customer with active bookings." });

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}