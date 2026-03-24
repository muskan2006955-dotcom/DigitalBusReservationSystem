using Microsoft.AspNetCore.Mvc;
using BusReservationSystem.Models;
using System.Linq;

namespace BusReservationSystem.Controllers
{
    public class CancellationPolicyController : Controller
    {
        private readonly BusReserveDbContext _context;

        public CancellationPolicyController(BusReserveDbContext context)
        {
            _context = context;
        }

        // 1. List all policies
        public IActionResult Index()
        {
            var policies = _context.CancellationPolicies
                .OrderByDescending(p => p.MinimumDaysBeforeTravel)
                .ToList();
            return View(policies);
        }

        // 2. GET: Create Policy
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create Policy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CancellationPolicy policy)
        {
            if (ModelState.IsValid)
            {
                _context.CancellationPolicies.Add(policy);
                _context.SaveChanges();
                TempData["Success"] = "Policy created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(policy);
        }

        // 3. GET: Edit Policy
        public IActionResult Edit(int id)
        {
            var policy = _context.CancellationPolicies.Find(id);
            if (policy == null)
            {
                return NotFound();
            }
            return View(policy);
        }

        // POST: Edit Policy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CancellationPolicy policy)
        {
            if (id != policy.PolicyId) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(policy);
                _context.SaveChanges();
                TempData["Success"] = "Policy updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(policy);
        }

        // 4. Delete Policy
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var policy = _context.CancellationPolicies.Find(id);
            if (policy != null)
            {
                _context.CancellationPolicies.Remove(policy);
                _context.SaveChanges();
                TempData["Success"] = "Policy removed.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}