using Microsoft.AspNetCore.Mvc;
using BusReservationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BusReservationSystem.Controllers
{
    public class AdminProfileController : Controller
    {
        private readonly BusReserveDbContext _context;

        public AdminProfileController(BusReserveDbContext context)
        {
            _context = context;
        }

        // GET: Profile Index (Contains Edit Form in Tabs)
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Login");

            var admin = await _context.Employees.FindAsync(int.Parse(userId));
            return View(admin);
        }

        // POST: Update Profile Info (Same Page Logic)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInfo(Employee model, IFormFile ProfilePic)
        {
            var admin = await _context.Employees.FindAsync(model.EmployeeId);

            if (admin != null)
            {
                if (ProfilePic != null && ProfilePic.Length > 0)
                {
                    // 1. Unique FileName create karein
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePic.FileName);

                    // 2. Path ko theek karein (images/profiles)
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");

                    // Agar folder nahi bana hua toh bana den
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string fullPath = Path.Combine(folderPath, fileName);

                    // 3. File save karein
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await ProfilePic.CopyToAsync(stream);
                    }

                    // 4. Purani image delete karne ka logic (Optional but Good)
                    if (!string.IsNullOrEmpty(admin.ProfilePicture))
                    {
                        string oldPath = Path.Combine(folderPath, admin.ProfilePicture);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    admin.ProfilePicture = fileName;
                }

                // Baki details update karein
                admin.FirstName = model.FirstName;
                admin.LastName = model.LastName;
                admin.PhoneNumber = model.PhoneNumber;
                admin.Age = model.Age;
                admin.Address = model.Address;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Profile Updated Successfully!";
            }
            return RedirectToAction("Index");
        }
        // GET: Change Password (Separate Page)
        public IActionResult Security()
        {
            return View();
        }

        // POST: Change Password Logic
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword.Length < 4)
            {
                TempData["Error"] = "New password must be at least 4 characters long!";
                return View("Security");
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match!";
                return View("Security");
            }

            var userId = HttpContext.Session.GetString("UserID");
            var admin = await _context.Employees.FindAsync(int.Parse(userId));

            if (admin != null && admin.Password == currentPassword)
            {
                admin.Password = newPassword;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Password updated successfully!";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Current password is incorrect!";
            return View("Security");
        }
    }
}