using BusReservationSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace BusReservationSystem.Controllers
{
    public class ManageEmployeeController : Controller
    {
        public ManageEmployeeController(BusReserveDbContext context)
        {
            Context = context;
        }

        public BusReserveDbContext Context { get; }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        
        public IActionResult Details(int id)
        {
            var data=Context.Employees.Find(id);
            return View(data);
        }
        [HttpPost]
        public IActionResult Register(Employee employee)
        {
            var data = Context.Employees.Add(employee);
            Context.SaveChanges();
            return RedirectToAction("EmployeeDashboard","Login");
        }


        public IActionResult ViewEmployees(string searchTerm)
        {
            // Base Query: Admin ko nikaal kar baaqi sab employees
            var query = Context.Employees.Where(e => e.Role != "Admin").AsQueryable();

            // Agar search term di gayi hai
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(e =>
                    e.FirstName.ToLower().Contains(searchTerm) ||
                    e.LastName.ToLower().Contains(searchTerm) ||
                    e.Email.ToLower().Contains(searchTerm) ||
                    e.PhoneNumber.Contains(searchTerm) ||
                    e.Role.ToLower().Contains(searchTerm)
                );
            }

            var data = query.ToList();
            return View(data);
        }
        public IActionResult Edit(int id)
        {
            var data = Context.Employees.Find(id);
            
            return View(data);
        }
        [HttpPost]
        public IActionResult Edit(Employee emp)
        {
            Context.Employees.Update(emp);
            Context.SaveChanges();
            return RedirectToAction("ViewEmployees");
        }
        public IActionResult Delete(int id)
        {
            var ticket = Context.Employees.Find(id);

            if (ticket == null)
                return NotFound();

            Context.Employees.Remove(ticket);
            Context.SaveChanges();

            TempData["SuccessMessage"] = $"Ticket #{id} deleted successfully.";
            return RedirectToAction("ViewEmployees"); // ya Index, depending on your page
        }
    }

}
