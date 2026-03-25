using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusReservationSystem.Models;
using Microsoft.AspNetCore.Hosting;

namespace BusReservationSystem.Controllers
{
    public class EmployeeProfileController : Controller
    {
        private readonly BusReserveDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Constructor mein context aur environment dono inject karein
        public EmployeeProfileController(BusReserveDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // 1. Profile View
        public async Task<IActionResult> Index()
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            int loggedInId = int.Parse(userIdStr);
            var employee = await _context.Employees.FindAsync(loggedInId);

            if (employee == null) return NotFound();
            return View(employee);
        }

        // 2. Upload Profile Picture (Fixed Path Logic)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(IFormFile ProfilePic)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            if (ProfilePic != null && ProfilePic.Length > 0)
            {
                // WebRootPath use karne se wwwroot ka sahi path milta hai
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string folder = Path.Combine(wwwRootPath, "images", "profiles");

                // Agar folder nahi bana hua toh bana den
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                // Unique FileName create karen
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePic.FileName);
                string filePath = Path.Combine(folder, fileName);

                // File ko physical folder mein save karen
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePic.CopyToAsync(stream);
                }

                // Database mein record update karen
                int loggedInId = int.Parse(userIdStr);
                var employee = await _context.Employees.FindAsync(loggedInId);

                if (employee != null)
                {
                    // Purani file delete karna (Safai ke liye)
                    if (!string.IsNullOrEmpty(employee.ProfilePicture))
                    {
                        string oldPath = Path.Combine(folder, employee.ProfilePicture);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    employee.ProfilePicture = fileName;
                    _context.Update(employee);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Profile picture updated successfully!";
                }
            }
            else
            {
                TempData["Error"] = "Please select a valid image file.";
            }

            return RedirectToAction(nameof(Index));
        }

        // 3. Update Personal Info
        // 1. Personal Info Update (Phone, Address, etc.)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInfo(Employee emp)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            int loggedInId = int.Parse(userIdStr);
            var existingEmp = await _context.Employees.FindAsync(loggedInId);

            if (existingEmp != null)
            {
                existingEmp.PhoneNumber = emp.PhoneNumber;
                existingEmp.Address = emp.Address;
                existingEmp.Age = emp.Age;
                existingEmp.Gender = emp.Gender;
                existingEmp.FirstName = emp.FirstName; // Agar naam bhi update karwana ho
                existingEmp.LastName = emp.LastName;

                _context.Update(existingEmp);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Profile details updated successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // Change Password ka alag page dikhane ke liye
        public IActionResult ChangePassword()
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            return View();
        }
        // 2. Password Change Logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login");

            int loggedInId = int.Parse(userIdStr);
            var employee = await _context.Employees.FindAsync(loggedInId);

            if (employee != null)
            {
                // Check current password
                if (employee.Password != currentPassword)
                {
                    TempData["Error"] = "Purana password galat hai!";
                    return RedirectToAction(nameof(Index));
                }

                // Check if new passwords match
                if (newPassword != confirmPassword)
                {
                    TempData["Error"] = "Naya password aur Confirm password match nahi kar rahe!";
                    return RedirectToAction(nameof(Index));
                }

                employee.Password = newPassword;
                _context.Update(employee);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Password kamyabi se badal diya gaya hai!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}