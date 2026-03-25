using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusReservationSystem.Models;

namespace BusReservationSystem.Controllers
{
    public class AdminViewBookingController : Controller
    {
        private readonly BusReserveDbContext _context;

        public AdminViewBookingController(BusReserveDbContext context)
        {
            _context = context;
        }

        // 1. Index: System ki tamam bookings
        public async Task<IActionResult> Index(string searchTerm, DateTime? travelDate)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Bus)
                .Include(b => b.BookedByNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(b => b.Customer.CustomerName.ToLower().Contains(searchTerm)
                                      || b.Customer.PhoneNumber.Contains(searchTerm)
                                      || b.Customer.IdproofNumber.Contains(searchTerm)
                                      || b.BookedByNavigation.FirstName.ToLower().Contains(searchTerm));
            }

            if (travelDate.HasValue)
            {
                var dateOnly = DateOnly.FromDateTime(travelDate.Value);
                query = query.Where(b => b.TravelDate == dateOnly);
            }

            var results = await query.OrderByDescending(b => b.BookingId).ToListAsync();
            return View(results);
        }

        // 2. Global Search
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Bus)
                .Include(b => b.BookedByNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string term = searchTerm.Trim().ToLower();
                query = query.Where(b =>
                    b.BookingId.ToString() == term ||
                    b.Customer.CustomerName.ToLower().Contains(term) ||
                    b.Customer.PhoneNumber.Contains(term) ||
                    b.BookedByNavigation.FirstName.ToLower().Contains(term)
                );
            }

            var results = await query.OrderByDescending(b => b.BookingId).ToListAsync();
            return View(results);
        }

        // 3. Cancelled Bookings Index
        public async Task<IActionResult> CancelledIndex(string searchTerm, DateTime? cancelDate)
        {
            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Bus)
                .Include(b => b.BookedByNavigation)
                .Where(b => b.BookingStatus == "Cancelled");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(b => b.Customer.CustomerName.ToLower().Contains(searchTerm)
                                      || b.Customer.PhoneNumber.Contains(searchTerm)
                                      || b.BookedByNavigation.FirstName.ToLower().Contains(searchTerm));
            }

            if (cancelDate.HasValue)
            {
                query = query.Where(b => b.CancellationDate.HasValue &&
                                         b.CancellationDate.Value.Date == cancelDate.Value.Date);
            }

            var results = await query.OrderByDescending(b => b.CancellationDate).ToListAsync();
            return View(results);
        }

        // 4. GET: Confirm Cancellation (Preview Page)
        public async Task<IActionResult> ConfirmCancellation(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Bus)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            DateTime travelDateTime = booking.TravelDate.ToDateTime(TimeOnly.MinValue);
            int daysRemaining = (travelDateTime.Date - DateTime.Now.Date).Days;

            var policy = await _context.CancellationPolicies
                .Where(p => p.MinimumDaysBeforeTravel <= daysRemaining)
                .OrderByDescending(p => p.MinimumDaysBeforeTravel)
                .FirstOrDefaultAsync();

            decimal totalAmount = booking.FinalAmount ?? 0;
            decimal deduction = 0;
            decimal refund = totalAmount;

            if (policy != null)
            {
                deduction = (totalAmount * (policy.DeductionPercentage ?? 0)) / 100;
                refund = totalAmount - deduction;
            }

            ViewBag.Deduction = deduction;
            ViewBag.Refund = refund;
            ViewBag.PolicyDays = policy?.MinimumDaysBeforeTravel ?? 0;
            ViewBag.Percentage = policy?.DeductionPercentage ?? 0;

            return View(booking);
        }

        // 5. POST: Final Cancel Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            DateTime travelDateTime = booking.TravelDate.ToDateTime(TimeOnly.MinValue);
            int daysRemaining = (travelDateTime.Date - DateTime.Now.Date).Days;

            var policy = await _context.CancellationPolicies
                .Where(p => p.MinimumDaysBeforeTravel <= daysRemaining)
                .OrderByDescending(p => p.MinimumDaysBeforeTravel)
                .FirstOrDefaultAsync();

            decimal refund = booking.FinalAmount ?? 0;
            if (policy != null)
            {
                decimal deduction = (refund * (policy.DeductionPercentage ?? 0)) / 100;
                refund -= deduction;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var seat = await _context.Seats.FirstOrDefaultAsync(s =>
                        s.BusId == booking.BusId && s.SeatNumber == booking.SeatNumber);
                    if (seat != null) seat.SeatStatus = "Available";

                    booking.BookingStatus = "Cancelled";
                    booking.CancellationDate = DateTime.Now;
                    booking.RefundAmount = refund;

                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = "Booking cancelled successfully.";
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Cancellation failed.";
                }
            }

            return RedirectToAction(nameof(CancelledIndex));
        }

        // 6. Delete Permanent
        [HttpPost]
        [Route("AdminViewBooking/DeleteConfirmed/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return Json(new { success = false, message = "Not Found" });

            var seat = await _context.Seats.FirstOrDefaultAsync(s => s.BusId == booking.BusId && s.SeatNumber == booking.SeatNumber);
            if (seat != null) seat.SeatStatus = "Available";

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}