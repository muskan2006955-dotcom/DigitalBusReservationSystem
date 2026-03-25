namespace BusReservationSystem.Models
{
    public class EmployeeDashboardViewModel
    {
        public string EmployeeName { get; set; } = null!;

        // Yeh line add karein:
        public string? ProfilePicture { get; set; }

        public int TotalMyCustomers { get; set; }
        public int TotalMyBookings { get; set; }
        public decimal TotalMyEarnings { get; set; }
        public int TotalBusesAvailable { get; set; }

        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
        public List<Customer> RecentCustomers { get; set; } = new List<Customer>();
    }
}