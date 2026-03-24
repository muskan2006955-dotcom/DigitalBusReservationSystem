using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BusReservationSystem.Models;

namespace BusReservationSystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly BusReserveDbContext _context;

        public BookingController(BusReserveDbContext context)
        {
            _context = context;
        }
        private async Task LogActivity(string action, string module, string details)
        {
            var log = new SystemLog
            {
                AdminEmail = HttpContext.Session.GetString("Name") ?? "Admin",
                Action = action,
                Module = module,
                Details = details,
                Timestamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.SystemLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        // 1. Index: Sirf login employee ki apni bookings dikhaye
        public async Task<IActionResult> Index()
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            int loggedInId = int.Parse(userIdStr);

            var bookings = await _context.Bookings
                .Include(b => b.Bus)
                .Include(b => b.Customer)
                .Where(b => b.BookedBy == loggedInId)
                .OrderByDescending(b => b.BookingId)
                .ToListAsync();

            return View(bookings);
        }

      
        // 2. Create (GET): Bus Number ke sath Route (Location) dikhane ke liye
        [HttpGet]
        public IActionResult Create()
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            int loggedInId = int.Parse(userIdStr);

            // Bus List with Locations: Bus Number + Starting Point + Destination
            // Hum ek anonymous object banayenge taake SelectList mein formatted text dikh sake
            var busesWithLocation = _context.Buses.Select(b => new
            {
                BusId = b.BusId,
                DisplayText = b.BusNumber + " | " + b.StartingPoint + " to " + b.DestinationPoint
            }).ToList();

            ViewBag.BusList = new SelectList(busesWithLocation, "BusId", "DisplayText");

            // Fetch Customers registered by this employee
            var myCustomers = _context.Customers
                .Where(c => c.RegisteredBy == loggedInId)
                .ToList();

            // Dropdown 1: Names ke liye
            ViewBag.CustomerNameList = new SelectList(myCustomers, "CustomerId", "CustomerName");

            // Dropdown 2: CNIC/Idproof ke liye
            ViewBag.CustomerCnicList = new SelectList(myCustomers, "CustomerId", "IdproofNumber");

            return View();
        }
        // 3. Create (POST): Booking save karna
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            int loggedInId = int.Parse(userIdStr);
            booking.BookedBy = loggedInId;

            // Auto-fill route info from Bus
            var bus = await _context.Buses.FindAsync(booking.BusId);
            if (bus != null)
            {
                booking.StartingPoint = bus.StartingPoint;
                booking.DestinationPoint = bus.DestinationPoint;
            }

            // Seat status check
            var seat = await _context.Seats.FirstOrDefaultAsync(s =>
                s.BusId == booking.BusId && s.SeatNumber == booking.SeatNumber);

            if (seat == null)
                ModelState.AddModelError("", "Seat does not exist.");
            else if (seat.SeatStatus != "Available")
                ModelState.AddModelError("", "Seat is already booked.");

            if (ModelState.IsValid)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        if (seat != null) seat.SeatStatus = "Booked";

                        booking.BookingDate = DateTime.Now;
                        booking.BookingStatus = "Booked";

                        _context.Add(booking);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        await LogActivity("Insert", "Booking", "New seat booked");
                        return RedirectToAction(nameof(BookedCustomers));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "Error: " + ex.Message);
                    }
                }
            }

            // Agar error aaye toh dropdowns dobara bharein (filtered)
            ViewBag.BusId = new SelectList(_context.Buses, "BusId", "BusNumber", booking.BusId);
            var myCustomers = _context.Customers.Where(c => c.RegisteredBy == loggedInId).ToList();
            ViewBag.CustomerId = new SelectList(myCustomers, "CustomerId", "CustomerName", booking.CustomerId);

            return View(booking);
        }

        // 4. Search: Sirf apni bookings mein search
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            int loggedInId = int.Parse(userIdStr);
            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Bus)
                .Where(b => b.BookedBy == loggedInId);

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return View(await query.OrderByDescending(b => b.BookingId).Take(10).ToListAsync());
            }

            string term = searchTerm.Trim().ToLower();
            var results = await query.Where(b =>
                b.BookingId.ToString() == term ||
                b.Customer.IdproofNumber.ToLower().Contains(term) ||
                b.Customer.PhoneNumber.Contains(term)
            ).ToListAsync();

            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> CalculateFare(int busId, int customerId)
        {
            var bus = await _context.Buses.FindAsync(busId);
            var customer = await _context.Customers.FindAsync(customerId);
            if (bus == null || customer == null) return Json(new { error = "Data not found" });

            var priceRule = await _context.PriceLists
                .FirstOrDefaultAsync(p => p.BusType.Trim().ToLower() == bus.BusType.Trim().ToLower());

            if (priceRule == null) return Json(new { error = "No Price Rule found." });

            decimal baseFare = (decimal)(bus.DistanceInKm) * (priceRule.PricePerKm ?? 0);
            decimal discount = 0;
            if (customer.Age < 5) discount = baseFare;
            else if (customer.Age >= 5 && customer.Age <= 12) discount = baseFare * 0.50m;
            else if (customer.Age > 50) discount = baseFare * 0.30m;

            decimal taxAmount = (baseFare * (priceRule.TaxPercentage ?? 0)) / 100;
            decimal finalAmount = (baseFare - discount) + taxAmount;

            return Json(new
            {
                baseFare = baseFare.ToString("0.00"),
                discount = discount.ToString("0.00"),
                taxAmount = taxAmount.ToString("0.00"),
                finalAmount = finalAmount.ToString("0.00")
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSeats(int busId)
        {
            var seats = await _context.Seats
                .Where(s => s.BusId == busId && s.SeatStatus == "Available")
                .OrderBy(s => s.SeatNumber)
                .Select(s => new { val = s.SeatNumber ?? 0 })
                .ToListAsync();
            return Json(seats);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // 1. Session aur Login Check
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            int loggedInId = int.Parse(userIdStr);

            var booking = await _context.Bookings
                .Include(b => b.Bus)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            // 2. Security Check: Kya ye booking isi employee ki hai?
            if (booking == null || booking.BookedBy != loggedInId) return NotFound();

            // 3. Customers Dropdown (Formatted for CNIC and Name)
            var myCustomers = _context.Customers
                .Where(c => c.RegisteredBy == loggedInId)
                .ToList();

            // View mein ddlName aur ddlCnic ke liye lists
            ViewBag.CustomerNameList = new SelectList(myCustomers, "CustomerId", "CustomerName", booking.CustomerId);
            ViewBag.CustomerCnicList = new SelectList(myCustomers, "CustomerId", "IdproofNumber", booking.CustomerId);

            // 4. Bus Dropdown (Formatted with Starting & Destination Point)
            var buses = _context.Buses.Select(b => new {
                Id = b.BusId,
                Text = b.BusNumber + " | " + b.StartingPoint + " to " + b.DestinationPoint
            }).ToList();

            ViewBag.BusId = new SelectList(buses, "Id", "Text", booking.BusId);

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            int loggedInId = int.Parse(userIdStr);

            if (id != booking.BookingId) return NotFound();

            try
            {
                // Purani booking nikalen track kiye bagair
                var original = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.BookingId == id);

                // Security check post back par bhi
                if (original == null || original.BookedBy != loggedInId) return Unauthorized();

                // 1. Seat change logic
                if (original.BusId != booking.BusId || original.SeatNumber != booking.SeatNumber)
                {
                    // Purani seat Available karen
                    var oldSeat = await _context.Seats.FirstOrDefaultAsync(s => s.BusId == original.BusId && s.SeatNumber == original.SeatNumber);
                    if (oldSeat != null) oldSeat.SeatStatus = "Available";

                    // Nayi seat Booked karen
                    var newSeat = await _context.Seats.FirstOrDefaultAsync(s => s.BusId == booking.BusId && s.SeatNumber == booking.SeatNumber);
                    if (newSeat != null)
                    {
                        if (newSeat.SeatStatus != "Available")
                        {
                            ModelState.AddModelError("", "New seat is already taken.");
                            throw new Exception("Seat taken");
                        }
                        newSeat.SeatStatus = "Booked";
                    }
                }

                // 2. Data Maintain rakhen
                booking.BookedBy = loggedInId; // Ensure employee ID doesn't change
                booking.BookingDate = original.BookingDate; // Purani date barkarar rakhen

                _context.Update(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                // Error par dropdowns dobara bharen (filtered)
                var myCustomers = _context.Customers.Where(c => c.RegisteredBy == loggedInId).ToList();
                ViewBag.CustomerId = new SelectList(myCustomers, "CustomerId", "CustomerName", booking.CustomerId);

                var buses = _context.Buses.Select(b => new { Id = b.BusId, Text = b.BusNumber }).ToList();
                ViewBag.BusId = new SelectList(buses, "Id", "Text", booking.BusId);

                return View(booking);
            }
        }
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Bus)
                .Include(b => b.Customer)
                .Include(b => b.BookedByNavigation) // Yeh line lazmi honi chahiye
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }
        [HttpPost]
        [Route("Booking/DeleteConfirmed/{id}")] // Yeh route lazmi add karein
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            // 1. Seat available karen
            var seat = await _context.Seats
                .FirstOrDefaultAsync(s => s.BusId == booking.BusId && s.SeatNumber == booking.SeatNumber);
            if (seat != null) seat.SeatStatus = "Available";

            // 2. Delete
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return Json(new { success = true }); // JSON response bhejein
        }
        // [HttpPost] hata dein taake URL se search ho sake
        // 5. Cancel Booking Action: Status update aur seat free karna
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");
            int loggedInId = int.Parse(userIdStr);

            var booking = await _context.Bookings
                .Include(b => b.Bus)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.BookedBy == loggedInId);

            if (booking == null) return NotFound();

            // Policy check (Optional: Pehli policy utha lo)
            var policy = await _context.CancellationPolicies.FirstOrDefaultAsync();
            decimal refund = booking.FinalAmount ?? 0;

            if (policy != null && policy.DeductionPercentage.HasValue)
            {
                decimal deduction = (refund * policy.DeductionPercentage.Value) / 100;
                refund -= deduction;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Seat ko wapas Available karein
                    var seat = await _context.Seats.FirstOrDefaultAsync(s =>
                        s.BusId == booking.BusId && s.SeatNumber == booking.SeatNumber);
                    if (seat != null) seat.SeatStatus = "Available";

                    // 2. Booking status update karein
                    booking.BookingStatus = "Cancelled";
                    booking.CancellationDate = DateTime.Now;
                    booking.RefundAmount = refund;

                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    await LogActivity("Cancel", "Booking", "Booking cancelled by admin");
                    TempData["Success"] = "Booking Cancelled Successfully!";
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Error while cancelling booking.";
                }
            }

            return RedirectToAction(nameof(CancelledIndex));
        }

        // 6. Cancelled Bookings Index: Sirf cancelled bookings ki list
        public async Task<IActionResult> CancelledIndex(string searchTerm, DateTime? cancelDate)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            int loggedInId = int.Parse(userIdStr);

            // Sirf Cancelled status wali aur current employee ki bookings
            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Bus)
                .Where(b => b.BookingStatus == "Cancelled" && b.BookedBy == loggedInId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(b => b.Customer.CustomerName.ToLower().Contains(searchTerm)
                                      || b.Customer.IdproofNumber.Contains(searchTerm));
            }

            if (cancelDate.HasValue)
            {
                // CancellationDate nullable DateTime hota hai, isliye hum sirf Date match karenge
                query = query.Where(b => b.CancellationDate.HasValue &&
                                        b.CancellationDate.Value.Date == cancelDate.Value.Date);
            }

            var results = await query.OrderByDescending(b => b.CancellationDate).ToListAsync();
            return View(results);
        }        // SIRF YE EK METHOD RAKHEIN
        public async Task<IActionResult> BookedCustomers(string searchTerm, DateTime? travelDate)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            int loggedInId = int.Parse(userIdStr);

            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Bus)
                .Where(b => b.BookedBy == loggedInId && b.BookingStatus == "Booked");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(b => b.Customer.CustomerName.ToLower().Contains(searchTerm)
                                      || b.Customer.IdproofNumber.Contains(searchTerm));
            }

            if (travelDate.HasValue)
            {
                var dateOnly = DateOnly.FromDateTime(travelDate.Value);
                query = query.Where(b => b.TravelDate == dateOnly);
            }

            var results = await query.OrderByDescending(b => b.TravelDate).ToListAsync();
            return View(results);
        }
    }
}