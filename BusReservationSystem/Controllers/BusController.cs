using BusReservationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusReservationSystem.Controllers
{
    public class BusController : Controller
    {
        private readonly BusReserveDbContext context;

        public BusController(BusReserveDbContext context)
        {
            this.context = context;
        }

        // List
        // List with Search Filter
        public IActionResult ViewBus(string searchTerm)
        {
            var query = context.Buses.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                // Bus Number, Code, ya Route (Starting/Destination) se search
                query = query.Where(b => b.BusNumber.ToLower().Contains(searchTerm) ||
                                         b.BusCode.ToLower().Contains(searchTerm) ||
                                         b.StartingPoint.ToLower().Contains(searchTerm) ||
                                         b.DestinationPoint.ToLower().Contains(searchTerm));
            }

            var data = query.ToList();
            return View(data);
        }

        // GET Create
        public IActionResult Create()
        {
            return View();
        }

        // POST Create
        [HttpPost]
        public IActionResult Create(Bus bus)
        {
            if (ModelState.IsValid)
            {
                // 🔹 Get last BusId
                int lastId = context.Buses
                    .OrderByDescending(b => b.BusId)
                    .Select(b => b.BusId)
                    .FirstOrDefault();

                int newNumber = lastId + 1;

                // 🔹 Auto Generate Code & Number
                bus.BusCode = "BUS-" + newNumber.ToString("D3");
                bus.BusNumber = "KHI-" + (1000 + newNumber);

                bus.Status = "Active";

                context.Buses.Add(bus);
                context.SaveChanges();

                // 🔥 AUTO SEAT GENERATION
                for (int i = 1; i <= bus.TotalSeats; i++)
                {
                    Seat seat = new Seat
                    {
                        BusId = bus.BusId,
                        SeatNumber = i,
                        SeatStatus = "Available"
                    };

                    context.Seats.Add(seat);
                }

                context.SaveChanges();

                return RedirectToAction("ViewBus");
            }

            return View(bus);
        }
        // GET: Edit Bus
        public IActionResult Edit(int id)
        {
            var bus = context.Buses.Find(id);
            if (bus == null)
            {
                return NotFound();
            }
            return View(bus);
        }

        // POST: Edit Bus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Bus bus)
        {
            if (id != bus.BusId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Status aur baaqi details update karein
                    context.Update(bus);
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!context.Buses.Any(e => e.BusId == bus.BusId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(ViewBus));
            }
            return View(bus);
        }
        public IActionResult Details(int id)
        {
            var data = context.Buses.Find(id);
            return View(data);
        }

        public IActionResult Delete(int id)
        {
            var data = context.Buses.Find(id);

            if (data == null)
            {
                return NotFound();
            }

            // 🔥 Pehle Seats delete karo
            var seats = context.Seats.Where(s => s.BusId == id).ToList();
            context.Seats.RemoveRange(seats);

            // Phir Bus delete karo
            context.Buses.Remove(data);

            context.SaveChanges();

            return RedirectToAction("ViewBus");
        }
    }
}