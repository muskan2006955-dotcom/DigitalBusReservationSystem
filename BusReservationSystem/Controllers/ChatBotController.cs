using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace BusReservationSystem.Controllers
{
    public class ChatBotController : Controller
    {
        // 1. Aapki API Key
        private readonly string _apiKey = "AIzaSyDL6tD97SFZfiExd7yAovB3RWbEB28rYPI";

        // 2. LATEST URL (Aapki list ke mutabiq v1beta aur gemini-2.5-flash)
        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetResponse(string userMessage)
        {
            if (string.IsNullOrEmpty(userMessage))
                return Json(new { reply = "Ji sir, puchiye?" });

            try
            {
                using (var client = new HttpClient())
                {
                    // Yahan hum AI ko uska role samjha rahe hain
                    var requestBody = new
                    {
                        system_instruction = new
                        {
                            parts = new
                            {
                                text = "You are the Galaxy AI Assistant for a Bus Reservation System. " +
                                               "Help Admins with fleet management, route scheduling, and revenue reports. " +
                                               "Help Employees with ticket booking, seat availability, and passenger queries. " +
                                               "Keep answers professional, short, and technical where needed. roman urdu my hi sirf bt krna"
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

                    return Json(new { reply = "Technical Error: " + jsonResponse });
                }
            }
            catch (Exception ex)
            {
                return Json(new { reply = "Error: " + ex.Message });
            }
        }
    }
}