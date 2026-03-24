using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using BusReservationSystem.Models; // Aapka models namespace
using Microsoft.EntityFrameworkCore;

namespace BusReservationSystem.Controllers
{
    public class EmployeeChatBot : Controller
    {
        private readonly BusReserveDbContext _context; // Aapka DB Context
        private readonly string _apiKey = "AIzaSyDL6tD97SFZfiExd7yAovB3RWbEB28rYPI";
        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        public EmployeeChatBot(BusReserveDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> GetResponse(string userMessage)
        {
            // 1. Session se UserID uthana (Jaisa aapne BookingController mein kiya)
            var userIdStr = HttpContext.Session.GetString("UserID");

            if (string.IsNullOrEmpty(userIdStr))
            {
                return Json(new { reply = "Aapka session khatam ho gaya hai. Meharbani karke dubara login karein." });
            }

            int loggedInId = int.Parse(userIdStr);
            string lowerMsg = userMessage.ToLower();
            string employeeDataInfo = "";

            try
            {
                // --- STEP 1: Employee ka apna Revenue aur Booking Data nikalna ---
                if (lowerMsg.Contains("revenue") || lowerMsg.Contains("kamayi") || lowerMsg.Contains("booking"))
                {
                    // Sirf is employee ki Active (Booked) bookings nikalna
                    var myBookings = await _context.Bookings
                        .Where(b => b.BookedBy == loggedInId && b.BookingStatus == "Booked")
                        .ToListAsync();

                    decimal totalRevenue = myBookings.Sum(b => b.FinalAmount ?? 0);
                    int totalBookings = myBookings.Count;

                    // Aaj ki kamayi (Today's Revenue)
                    var today = DateTime.Now.Date;
                    decimal todayRevenue = myBookings
                        .Where(b => b.BookingDate.HasValue && b.BookingDate.Value.Date == today)
                        .Sum(b => b.FinalAmount ?? 0);

                    employeeDataInfo = $"[SYSTEM DATA: This employee (ID: {loggedInId}) has total {totalBookings} active bookings. " +
                                       $"Total Revenue: Rs. {totalRevenue}. Today's Revenue: Rs. {todayRevenue}.]";
                }

                // --- STEP 2: Gemini AI ko Context ke sath call karna ---
                using (var client = new HttpClient())
                {
                    var requestBody = new
                    {
                        system_instruction = new
                        {
                            parts = new
                            {
                                text = "You are Galaxy AI for Employees. Use this specific data to answer: " + employeeDataInfo +
                                       ". Only show data belonging to the logged-in employee. Keep it professional and helpful. sirf roman urdu my bt krna"
                            }
                        },
                        contents = new[] {
                            new { parts = new[] { new { text = userMessage } } }
                        }
                    };

                    var jsonPayload = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{_apiUrl}?key={_apiKey}", content);
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    using var doc = JsonDocument.Parse(jsonResponse);
                    if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                    {
                        var replyText = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                        return Json(new { reply = replyText });
                    }

                    return Json(new { reply = "Technical issue. Please try again." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { reply = "Data fetch error: " + ex.Message });
            }
        }
    }
}