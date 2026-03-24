using Microsoft.AspNetCore.Mvc;
using BusReservationSystem.Models;
namespace BusReservationSystem.Controllers;

public class PriceListController : Controller
{
    private readonly BusReserveDbContext _context;

    public PriceListController(BusReserveDbContext context)
    {
        _context = context;
    }

    // List all prices
    public IActionResult Index(string searchTerm)
    {
        var query = _context.PriceLists.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();
            // Bus Type ke mutabiq filter
            query = query.Where(p => p.BusType.ToLower().Contains(searchTerm));
        }

        var prices = query.ToList();
        return View(prices);
    }
    // Edit price (admin can update price or tax)
    public IActionResult Edit(int id)
    {
        var price = _context.PriceLists.FirstOrDefault(p => p.PriceId == id);
        if (price == null) return NotFound();

        return View(price);
    }

    [HttpPost]
    public IActionResult Edit(PriceList model)
    {
        if (!ModelState.IsValid) return View(model);

        var price = _context.PriceLists.FirstOrDefault(p => p.PriceId == model.PriceId);
        if (price == null) return NotFound();

        // update only price & tax (bus type fixed)
        price.PricePerKm = model.PricePerKm;
        price.TaxPercentage = model.TaxPercentage;
        price.EffectiveDate = model.EffectiveDate;

        _context.SaveChanges();
        return RedirectToAction("Index");
    }
    // GET: PriceList/Details/5
    public IActionResult Details(int id)
    {
        var price = _context.PriceLists.FirstOrDefault(p => p.PriceId == id);

        if (price == null)
        {
            return NotFound();
        }

        return View(price);
    }
}