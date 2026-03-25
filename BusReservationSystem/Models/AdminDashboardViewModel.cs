using System.Collections.Generic;

namespace BusReservationSystem.Models
{
    public class AdminDashboardViewModel
    {
        public string AdminName { get; set; } = "Admin";
        public string ProfilePicture { get; set; } = "default-user.png";

        public decimal TotalRevenue { get; set; }
        public int TotalBuses { get; set; }
        public int ActiveAgents { get; set; }
        public int TotalCancellations { get; set; }

        // Isay initialize karna zaroori hai null error se bachne ke liye
        public List<Booking> RecentSystemBookings { get; set; } = new List<Booking>();
    }
}